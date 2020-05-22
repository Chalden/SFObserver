// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FabricObserver.Observers.Utilities
{
    public class WindowsPerfCounters : IDisposable
    {
        private PerformanceCounter diskAverageQueueLengthCounter;
        private PerformanceCounter cpuTimePerfCounter;
        private PerformanceCounter memCommittedBytesPerfCounter;
        private IDictionary<string, PerformanceCounter> counters;
        private bool disposedValue;

        private Logger Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsPerfCounters"/> class.
        /// </summary>
        public WindowsPerfCounters()
        {
            _ = this.InitializePerfCounters();
            this.Logger = new Logger("Utilities");
        }

        public bool InitializePerfCounters()
        {
            try
            {
                counters = new Dictionary<string, PerformanceCounter>();
                counters.Add("Working Set - Private", new PerformanceCounter());
                counters.Add("IO Read Operations/sec", new PerformanceCounter());
                counters.Add("IO Write Operations/sec", new PerformanceCounter());
                counters.Add("IO Data Operations/sec", new PerformanceCounter());
                counters.Add("IO Read Bytes/sec", new PerformanceCounter());
                counters.Add("IO Write Bytes/sec", new PerformanceCounter());
                counters.Add("IO Data Bytes / sec", new PerformanceCounter());

                this.diskAverageQueueLengthCounter = new PerformanceCounter();
                this.cpuTimePerfCounter = new PerformanceCounter();
                this.memCommittedBytesPerfCounter = new PerformanceCounter();
            }
            catch (PlatformNotSupportedException)
            {
                return false;
            }
            catch (Exception e)
            {
                this.Logger.LogWarning(e.ToString());

                throw;
            }

            return true;
        }

        private float PerfCounterGetData(PerformanceCounter perfCounter, string procName, string counter)
        {
            string cat = "Process";

            try
            {
                perfCounter.CategoryName = cat;
                perfCounter.CounterName = counter;
                perfCounter.InstanceName = procName;

                return perfCounter.NextValue();
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException || e is PlatformNotSupportedException
                                               || e is System.ComponentModel.Win32Exception || e is UnauthorizedAccessException)
                {
                    this.Logger.LogError($"{cat} {counter} PerfCounter handled error: " + e);

                    // Don't throw.
                    return 0F;
                }

                this.Logger.LogError($"{cat} {counter} PerfCounter unhandled error: " + e);

                throw;
            }
        }

        internal float PerfCounterGetAverageDiskQueueLength(string instance)
        {
            string cat = "LogicalDisk";
            string counter = "Avg. Disk Queue Length";

            try
            {
                this.diskAverageQueueLengthCounter.CategoryName = cat;
                this.diskAverageQueueLengthCounter.CounterName = counter;
                this.diskAverageQueueLengthCounter.InstanceName = instance;

                return this.diskAverageQueueLengthCounter.NextValue();
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException || e is PlatformNotSupportedException
                    || e is System.ComponentModel.Win32Exception || e is UnauthorizedAccessException)
                {
                    this.Logger.LogError($"{cat} {counter} PerfCounter handled exception: " + e);

                    // Don't throw.
                    return 0F;
                }

                this.Logger.LogError($"{cat} {counter} PerfCounter unhandled exception: " + e);

                throw;
            }
        }

        internal float PerfCounterGetProcessorInfo(
            string counterName = null,
            string category = null,
            string instance = null)
        {
            string cat = "Processor";
            string counter = "% Processor Time";
            string inst = "_Total";

            try
            {
                if (!string.IsNullOrEmpty(category))
                {
                    cat = category;
                }

                if (!string.IsNullOrEmpty(counterName))
                {
                    counter = counterName;
                }

                if (!string.IsNullOrEmpty(instance))
                {
                    inst = instance;
                }

                this.cpuTimePerfCounter.CategoryName = cat;
                this.cpuTimePerfCounter.CounterName = counter;
                this.cpuTimePerfCounter.InstanceName = inst;

                return this.cpuTimePerfCounter.NextValue();
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException || e is PlatformNotSupportedException
                    || e is System.ComponentModel.Win32Exception || e is UnauthorizedAccessException)
                {
                    this.Logger.LogError($"{cat} {counter} PerfCounter handled error: " + e);

                    // Don't throw.
                    return 0F;
                }

                this.Logger.LogError($"{cat} {counter} PerfCounter unhandled error: " + e);

                throw;
            }
        }

        // Committed bytes.
        internal float PerfCounterGetMemoryInfoMb(
            string category = null,
            string counterName = null)
        {
            string cat = "Memory";
            string counter = "Committed Bytes";

            try
            {
                if (!string.IsNullOrEmpty(category))
                {
                    cat = category;
                }

                if (!string.IsNullOrEmpty(counterName))
                {
                    counter = counterName;
                }

                this.memCommittedBytesPerfCounter.CategoryName = cat;
                this.memCommittedBytesPerfCounter.CounterName = counter;

                return this.memCommittedBytesPerfCounter.NextValue() / 1024 / 1024;
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException || e is PlatformNotSupportedException
                    || e is System.ComponentModel.Win32Exception || e is UnauthorizedAccessException)
                {
                    this.Logger.LogError($"{cat} {counter} PerfCounter handled error: " + e);

                    // Don't throw.
                    return 0F;
                }

                this.Logger.LogError($"{cat} {counter} PerfCounter unhandled error: " + e);
                throw;
            }
        }

        internal float PerfCounterGetProcessPrivateWorkingSetMb(string procName)
        {
            string counter = "Working Set - Private";
            PerformanceCounter memProcessPrivateWorkingSetCounter = this.counters[counter];
            return PerfCounterGetData(memProcessPrivateWorkingSetCounter, procName, counter) / 1024 / 1024;
        }

        internal float PerfCounterGetProcessReadOpSec(string procName)
        {
            string counter = "IO Read Operations/sec";
            PerformanceCounter readOpSec = this.counters[counter];
            return PerfCounterGetData(readOpSec, procName, counter);
        }

        internal float PerfCounterGetProcessWriteOpSec(string procName)
        {
            string counter = "IO Write Operations/sec";
            PerformanceCounter writeOpSec = this.counters[counter];
            return PerfCounterGetData(writeOpSec, procName, counter);
        }

        internal float PerfCounterGetProcessDataOpSec(string procName)
        {
            string counter = "IO Data Operations/sec";
            PerformanceCounter dataOpSec = this.counters[counter];
            return PerfCounterGetData(dataOpSec, procName, counter);
        }

        internal float PerfCounterGetProcessReadBytesSec(string procName)
        {
            string counter = "IO Read Bytes/sec";
            PerformanceCounter readBytesSec = this.counters[counter];
            return PerfCounterGetData(readBytesSec, procName, counter);
        }

        internal float PerfCounterGetProcessWriteBytesSec(string procName)
        {
            string counter = "IO Write Bytes/sec";
            PerformanceCounter writeBytesSec = this.counters[counter];
            return PerfCounterGetData(writeBytesSec, procName, counter);
        }

        internal float PerfCounterGetProcessDataBytesSec(string procName)
        {
            string counter = "IO Data Bytes/sec";
            PerformanceCounter dataBytesSec = this.counters[counter];
            return PerfCounterGetData(dataBytesSec, procName, counter);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (this.disposedValue)
            {
                return;
            }

            if (disposing)
            {
                if (this.diskAverageQueueLengthCounter != null)
                {
                    this.diskAverageQueueLengthCounter.Dispose();
                    this.diskAverageQueueLengthCounter = null;
                }

                if (this.memCommittedBytesPerfCounter != null)
                {
                    this.memCommittedBytesPerfCounter.Dispose();
                    this.memCommittedBytesPerfCounter = null;
                }

                if (this.cpuTimePerfCounter != null)
                {
                    this.cpuTimePerfCounter.Dispose();
                    this.cpuTimePerfCounter = null;
                }

                if (this.counters != null)
                {
                    this.counters = null;
                }
            }

            this.disposedValue = true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
        }
    }
}
