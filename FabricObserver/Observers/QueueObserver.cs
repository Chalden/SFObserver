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

namespace FabricObserver.Observers
{
    public class QueueObserver : ObserverBase
    {
        public QueueObserver()
       : base(ObserverConstants.QueueObserverName)
        {
        }

        //Warning queue length
        public int WarningLength { get; set; }

        //Critical queue length
        public int CriticalLength { get; set; }

        //Max acceptable dequeue count
        public int MaxAcceptableDequeueCount { get; set; }

        //Queue name
        public string QueueName { get; set; }

        //Queue 
        private CloudQueue queue;

        private async Task Initialize(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var WarningLengthAsString = this.GetSettingParameterValue(
            ObserverConstants.QueueObserverConfigurationSectionName,
            ObserverConstants.QueueObserverWarningLength);

            this.WarningLength = Convert.ToInt32(WarningLengthAsString);

            var CriticalLengthAsString = this.GetSettingParameterValue(
            ObserverConstants.QueueObserverConfigurationSectionName,
            ObserverConstants.QueueObserverCriticalLength);

            this.CriticalLength = Convert.ToInt32(CriticalLengthAsString);

            var MaxAcceptableDequeueCountAsString = this.GetSettingParameterValue(
            ObserverConstants.QueueObserverConfigurationSectionName,
            ObserverConstants.QueueObserverMaxAcceptableDequeueCount);

            this.CriticalLength = Convert.ToInt32(MaxAcceptableDequeueCountAsString);

            this.QueueName = this.GetSettingParameterValue(
            ObserverConstants.QueueObserverConfigurationSectionName,
            ObserverConstants.QueueObserverQueueName);

        }

        public override async Task ObserveAsync(CancellationToken token)
        {
            if (this.RunInterval > TimeSpan.MinValue && DateTime.Now.Subtract(this.LastRunDateTime) < this.RunInterval)
            {
                return;
            }

            if (token.IsCancellationRequested)
            {
                return;
            }
            
            await this.Initialize(token).ConfigureAwait(true);

            //Queue connection
            this.queue = AzureStorageConnection.queueConnection(QueueName);

            await this.ReportAsync(token).ConfigureAwait(true);

            this.LastRunDateTime = DateTime.Now;
        }

        public override Task ReportAsync(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            //Fetch the queue attributes
            this.queue.FetchAttributes();

            //Retrieve the cached approximate message count
            int? cachedMessageCount = this.queue.ApproximateMessageCount;

            //Peek top 32 messages of the queue
            var messages = queue.PeekMessages(32).ToList();

            //Counter of messages with DequeueCount > max 
            int dequeueCounter = 0;
            
            foreach(var message in messages)
            {
                if (message.DequeueCount >= MaxAcceptableDequeueCount)
                {
                    dequeueCounter++;
                }
            }

            string healthMessage;
            HealthState state;

            if (cachedMessageCount.HasValue)
            {
                if (cachedMessageCount >= CriticalLength)
                {
                    healthMessage = $"{cachedMessageCount} messages in queue.\nMaximum acceptable length exceeded.";
                    state = HealthState.Error;
                }
                else if (cachedMessageCount >= WarningLength)
                {
                    healthMessage = $"{cachedMessageCount} messages in queue.\nYou have reached the warning threshold.";
                    state = HealthState.Warning;
                }
                else
                {
                    healthMessage = $"{cachedMessageCount} message(s) in queue.";
                    state = HealthState.Ok;
                }

                healthMessage = $"{healthMessage}\n{dequeueCounter} poison message(s).";
            }
            else {
                healthMessage = $"Impossible to retrieve message count.";
                state = HealthState.Warning;
            }

            HealthReport healthReport = new Utilities.HealthReport
            {
                Observer = this.ObserverName,
                ReportType = HealthReportType.Node,
                EmitLogEvent = true,
                NodeName = this.NodeName,
                HealthMessage = healthMessage,
                State = state,
            };

            this.HasActiveFabricErrorOrWarning = true;
            this.HealthReporter.ReportHealthToServiceFabric(healthReport);

            return Task.CompletedTask;
        }
    }
}
