using System;
using System.Collections.Generic;
using System.Fabric.Health;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using FabricObserver.Observers.Utilities;
using HealthReport = FabricObserver.Observers.Utilities.HealthReport;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Configuration;
using FabricObserver.Observers.Interfaces;

namespace FabricObserver.Observers
{
    public class QueueObserverLogic : IQueueObserverLogic
    {
        private readonly IQueueObserverAccessor QueueAccessor;

        public QueueObserverLogic(IQueueObserverAccessor queueAccessor)
        {
            this.QueueAccessor = queueAccessor;
        }

        //Warning queue length
        private int WarningLength { get; set; }

        //Critical queue length
        private int CriticalLength { get; set; }

        //Max acceptable dequeue count
        private int MaxAcceptableDequeueCount { get; set; }

        //Queue name
        private string QueueName { get; set; }

        private async Task Initialize(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            this.WarningLength = QueueAccessor.LoadWarningLength();
            this.CriticalLength = QueueAccessor.LoadCriticalLength();
            this.MaxAcceptableDequeueCount = QueueAccessor.LoadMaxAcceptableDequeueCount();
            this.QueueName = QueueAccessor.LoadQueueName();
        }

        public async Task ObserveAsync(CancellationToken token)
        {
            await this.Initialize(token).ConfigureAwait(true);
            try
            {
                QueueAccessor.OpenQueue(this.QueueName);
            }
            catch (Exception QueueName)
            {
                String healthMessage = $"Queue {this.QueueName} doesn't exist.";
                HealthState state = HealthState.Warning;

                QueueAccessor.SendReport(healthMessage, state);

                return;
            }

            await this.ReportAsync(token).ConfigureAwait(true);
        }

        public Task ReportAsync(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            //Fetch the queue attributes
            QueueAccessor.Refresh();

            //Retrieve the cached approximate message count
            int? cachedMessageCount = QueueAccessor.TryGetQueueLength();

            string healthMessage;
            HealthState state;

            if (!cachedMessageCount.HasValue)
            {
                healthMessage = "Impossible to retrieve message count.";
                state = HealthState.Warning;

                QueueAccessor.SendReport(healthMessage, state);

                return Task.CompletedTask;
            }

            if (cachedMessageCount == 0)
            {
                healthMessage = "Queue is empty.";
                state = HealthState.Warning;

                QueueAccessor.SendReport(healthMessage, state);

                return Task.CompletedTask;
            }

            //Peek top 32 messages of the queue
            List<CloudQueueMessage> messages = QueueAccessor.PeekMessages(32).ToList();

            //Counter of messages with DequeueCount > max 
            int dequeueCounter = 0;

            TimeSpan messageDuration = TimeSpan.Zero;

            foreach (CloudQueueMessage message in messages)
            {
                if (message.DequeueCount >= MaxAcceptableDequeueCount)
                {
                    dequeueCounter++;
                }

                if (message.InsertionTime == null)
                {
                    healthMessage = "Impossible to retrieve message insertion time.";
                    state = HealthState.Warning;

                    QueueAccessor.SendReport(healthMessage, state);

                    return Task.CompletedTask;
                }

                TimeSpan nextMessageDuration = DateTimeOffset.UtcNow.Subtract(message.InsertionTime.Value.UtcDateTime);
                if (messageDuration < nextMessageDuration)
                {
                    messageDuration = nextMessageDuration;
                }
            }

            string messageDurationAsString = $"Oldest message exists in queue since {messageDuration.Days} day(s), {messageDuration.Hours} hour(s), " +
            $"{messageDuration.Minutes} minute(s), {messageDuration.Seconds} second(s), {messageDuration.Milliseconds} millisecond(s).";

            if (cachedMessageCount >= CriticalLength)
            {
                healthMessage = $"{cachedMessageCount} messages in queue. Critical threshold reached.";
                state = HealthState.Error;
            }
            else if (cachedMessageCount >= WarningLength)
            {
                healthMessage = $"{cachedMessageCount} messages in queue. Warning threshold reached.";
                state = HealthState.Warning;
            }
            else
            {
                healthMessage = $"{cachedMessageCount} message(s) in queue.";
                state = HealthState.Ok;
            }
            healthMessage += $"\n{dequeueCounter} poison message(s).\n\n{messageDurationAsString}";

            QueueAccessor.SendReport(healthMessage, state);

            return Task.CompletedTask;
        }
    }
}
