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
    public class NetworkIOObserver : ObserverBase
    {
        public NetworkIOObserver()
       : base(ObserverConstants.QueueObserverName)
        {
        }

        public override async Task ObserveAsync(CancellationToken token)
        {
            if (this.RunInterval > TimeSpan.MinValue && DateTime.UtcNow.Subtract(this.LastRunDateTime) < this.RunInterval)
            {
                return;
            }

            if (token.IsCancellationRequested)
            {
                return;
            }
            this.LastRunDateTime = DateTime.UtcNow;
        }

        public override async Task ReportAsync(CancellationToken token)
        {

        }
    }
}