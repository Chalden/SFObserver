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
    public class AzureQueueObserverAccessor : IQueueObserverAccessor
    {
        private ObserverBase ObserverBase { get; }
        private CloudQueue CloudQueue { get; set; }

        public AzureQueueObserverAccessor(ObserverBase observerBase)
        {
            this.ObserverBase = observerBase;
        }

        public int LoadWarningLength()
        {
            var WarningLengthAsString = ObserverBase.GetSettingParameterValue(
                ObserverConstants.QueueObserverConfigurationSectionName,
                ObserverConstants.QueueObserverWarningLength);

            return Convert.ToInt32(WarningLengthAsString);
        }

        public int LoadCriticalLength()
        {
            var CriticalLengthAsString = ObserverBase.GetSettingParameterValue(
                ObserverConstants.QueueObserverConfigurationSectionName,
                ObserverConstants.QueueObserverCriticalLength);

            return Convert.ToInt32(CriticalLengthAsString);
        }

        public int LoadMaxAcceptableQueueCount()
        {
            var MaxAcceptableDequeueCountAsString = ObserverBase.GetSettingParameterValue(
                ObserverConstants.QueueObserverConfigurationSectionName,
                ObserverConstants.QueueObserverMaxAcceptableDequeueCount);

            return Convert.ToInt32(MaxAcceptableDequeueCountAsString);
        }

        public string LoadQueueName()
        {
            var queueName = ObserverBase.GetSettingParameterValue(
                ObserverConstants.QueueObserverConfigurationSectionName,
                ObserverConstants.QueueObserverQueueName);
            return queueName;
        }

        public void OpenQueue(string queueName)
        {
            this.CloudQueue = AzureStorageConnection.queueConnection(queueName);
            if (CloudQueue == null)
            {
                throw new Exception(queueName);
            }
        }

        public void Refresh()
        {
            this.CloudQueue.FetchAttributes();
        }

        public int? TryGetQueueLength()
        {
            return this.CloudQueue.ApproximateMessageCount;
        }

        public IEnumerable<CloudQueueMessage> PeekMessages(int length)
        {
            return CloudQueue.PeekMessages(length);
        }

        public void SendReport(string healthMessage, HealthState state)
        {
            HealthReport healthReport = new Utilities.HealthReport
            {
                Observer = this.ObserverBase.ObserverName,
                ReportType = HealthReportType.Node,
                EmitLogEvent = true,
                NodeName = this.ObserverBase.NodeName,
                HealthMessage = healthMessage,
                State = state,
            };

            ObserverBase.HasActiveFabricErrorOrWarning = true;
            ObserverBase.HealthReporter.ReportHealthToServiceFabric(healthReport);
        }
    }
}
