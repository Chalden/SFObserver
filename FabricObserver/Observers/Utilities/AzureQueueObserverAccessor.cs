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

namespace FabricObserver.Observers.Utilities
{
    public class AzureQueueObserverAccessor : IAzureQueueObserverAccessor
    {
        private readonly ObserverBase observerBase;
        private CloudQueue cloudQueue = null;

        public AzureQueueObserverAccessor(ObserverBase observerBase)
        {
            this.observerBase = observerBase;
        }

        public int LoadWarningLength()
        {
            var WarningLengthAsString = observerBase.GetSettingParameterValue(
                ObserverConstants.QueueObserverConfigurationSectionName,
                ObserverConstants.QueueObserverWarningLength);

            return Convert.ToInt32(WarningLengthAsString);
        }

        public int LoadCriticalLength()
        {
            var CriticalLengthAsString = observerBase.GetSettingParameterValue(
                ObserverConstants.QueueObserverConfigurationSectionName,
                ObserverConstants.QueueObserverCriticalLength);

            return Convert.ToInt32(CriticalLengthAsString);
        }

        public int LoadMaxAcceptableQueueCount()
        {
            var MaxAcceptableDequeueCountAsString = observerBase.GetSettingParameterValue(
                ObserverConstants.QueueObserverConfigurationSectionName,
                ObserverConstants.QueueObserverMaxAcceptableDequeueCount);

            return Convert.ToInt32(MaxAcceptableDequeueCountAsString);
        }

        public string LoadQueueName()
        {
            var queueName = observerBase.GetSettingParameterValue(
                ObserverConstants.QueueObserverConfigurationSectionName,
                ObserverConstants.QueueObserverQueueName);
            return queueName;
        }

        public void OpenQueue(string queueName)
        {
            this.cloudQueue = AzureStorageConnection.queueConnection(queueName);
            if (cloudQueue == null)
            {
                throw new Exception(queueName);
            }
        }

        public void Refresh()
        {
            this.cloudQueue.FetchAttributes();
        }

        public int? TryGetQueueLength()
        {
            return this.cloudQueue.ApproximateMessageCount;
        }

        public IEnumerable<CloudQueueMessage> PeekMessages(int length)
        {
            return cloudQueue.PeekMessages(length);
        }

        public void SendReport(string healthMessage, HealthState state)
        {
            HealthReport healthReport = new Utilities.HealthReport
            {
                Observer = this.observerBase.ObserverName,
                ReportType = HealthReportType.Node,
                EmitLogEvent = true,
                NodeName = this.observerBase.NodeName,
                HealthMessage = healthMessage,
                State = state,
            };

            observerBase.HasActiveFabricErrorOrWarning = true;
            observerBase.HealthReporter.ReportHealthToServiceFabric(healthReport);
        }
    }
}
