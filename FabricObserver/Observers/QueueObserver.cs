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

        //Maximum acceptable queue length
        public int maxLength { get; set; }

        //Critical queue length
        public int criticalLength { get; set; }

        //Queue
        public CloudQueue queue;

        public TimeSpan HealthReportTimeToLive { get; set; } = TimeSpan.FromDays(1);

        private async Task Initialize(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var varMaxLength = this.GetSettingParameterValue(
            ObserverConstants.QueueObserverConfigurationSectionName,
            ObserverConstants.QueueObserverMaxLength);

            this.maxLength = Convert.ToInt32(varMaxLength);

            var varCriticalLength = this.GetSettingParameterValue(
            ObserverConstants.QueueObserverConfigurationSectionName,
            ObserverConstants.QueueObserverCriticalLength);

            this.criticalLength = Convert.ToInt32(varCriticalLength);
        }

        public override async Task ObserveAsync(CancellationToken token)
        {
            Initialize(token);

            //Queue connection
            this.queue = AzureStorageConnection.queueConnection("queuetest");;

            ReportAsync(token);
        }

        public override async Task ReportAsync(CancellationToken token)
        {
            //Fetch the queue attributes
            this.queue.FetchAttributes();

            //Retrieve the cached approximate message count
            int? cachedMessageCount = this.queue.ApproximateMessageCount;

            string setHealthMessage = null;
            HealthState setState = HealthState.Ok;

            if (cachedMessageCount >= criticalLength && cachedMessageCount < maxLength)
            {
                setHealthMessage = $"" + cachedMessageCount + " messages in queue.\nYou have reached the warning threshold.";
                setState = HealthState.Warning;

            }
            else if (cachedMessageCount >= maxLength)
            {
                setHealthMessage = $"" + cachedMessageCount + " messages in queue.\nMaximum acceptable length exceeded.";
                setState = HealthState.Error;
            }
            else
            {
                setHealthMessage = $"" + cachedMessageCount + " message(s) in queue.";
            }

            HealthReport healthReport = new Utilities.HealthReport
            {
                Observer = this.ObserverName,
                ReportType = HealthReportType.Node,
                EmitLogEvent = true,
                NodeName = this.NodeName,
                HealthMessage = $"" + setHealthMessage,
                State = setState,
                HealthReportTimeToLive = this.RunInterval > TimeSpan.MinValue ? this.RunInterval : this.HealthReportTimeToLive,
            };
 
            this.HasActiveFabricErrorOrWarning = true;
            this.HealthReporter.ReportHealthToServiceFabric(healthReport);

            await this.ReportAsync(token).ConfigureAwait(true);
        }
    }
}
