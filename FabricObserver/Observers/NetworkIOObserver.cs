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
using System.Diagnostics;

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

            string pn = "le process";
            var readOpSec = new PerformanceCounter("Process", "IO Read Operations/sec", pn);
            var writeOpSec = new PerformanceCounter("Process", "IO Write Operations/sec", pn);
            var dataOpSec = new PerformanceCounter("Process", "IO Data Operations/sec", pn);
            var readBytesSec = new PerformanceCounter("Process", "IO Read Bytes/sec", pn);
            var writeByteSec = new PerformanceCounter("Process", "IO Write Bytes/sec", pn);
            var dataBytesSec = new PerformanceCounter("Process", "IO Data Bytes/sec", pn);

            var counters = new List<PerformanceCounter>
                {
                readOpSec,
                writeOpSec,
                dataOpSec,
                readBytesSec,
                writeByteSec,
                dataBytesSec
                };
        }

        public override async Task ReportAsync(CancellationToken token)
        {

        }
    }
}