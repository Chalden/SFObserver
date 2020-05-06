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
namespace FabricObserver.Observers.Interfaces
{
    public interface IAzureQueueObserverAccessor
    {
        int LoadWarningLength();
        int LoadCriticalLength();
        int LoadMaxAcceptableQueueCount();
        string LoadQueueName();
        void OpenQueue(string queueName);
        void Refresh();
        int? TryGetQueueLength();
        IEnumerable<CloudQueueMessage> PeekMessages(int length);
        void SendReport(string healthMessage, HealthState state);
    }
}
