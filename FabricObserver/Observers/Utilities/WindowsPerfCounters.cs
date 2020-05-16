// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Diagnostics;

namespace FabricObserver.Observers.Utilities
{
    public class WindowsPerfCounters : IDisposable
    {
        private PerformanceCounter diskAverageQueueLengthCounter;
        private PerformanceCounter cpuTimePerfCounter;
        private PerformanceCounter memCommittedBytesPerfCounter;
        private PerformanceCounter memProcessPrivateWorkingSetCounter;
        private PerformanceCounter readOpSec;
        private PerformanceCounter writeOpSec;
        private PerformanceCounter dataOpSec;
        private PerformanceCounter readBytesSec;
        private PerformanceCounter writeByteSec;
        private PerformanceCounter dataBytesSec;

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
                this.diskAverageQueueLengthCounter = new PerformanceCounter();
                this.cpuTimePerfCounter = new PerformanceCounter();
                this.memCommittedBytesPerfCounter = new PerformanceCounter();
                this.memProcessPrivateWorkingSetCounter = new PerformanceCounter();
                this.readOpSec = new PerformanceCounter();
                this.writeOpSec = new PerformanceCounter();
                this.dataOpSec = new PerformanceCounter();
                this.readBytesSec = new PerformanceCounter();
                this.writeByteSec = new PerformanceCounter();
                this.dataBytesSec = new PerformanceCounter();
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
            string cat = "Process";
            string counter = "Working Set - Private";

            try
            {
                this.memProcessPrivateWorkingSetCounter.CategoryName = cat;
                this.memProcessPrivateWorkingSetCounter.CounterName = counter;
                this.memProcessPrivateWorkingSetCounter.InstanceName = procName;

                return this.memProcessPrivateWorkingSetCounter.NextValue() / 1024 / 1024;
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

        internal float PerfCounterGetProcessReadOpSec(string procName)
        {
            string cat = "Process";
            string counter = "IO Read Operations/sec";

            try
            {
                this.readOpSec.CategoryName = cat;
                this.readOpSec.CounterName = counter;
                this.readOpSec.InstanceName = procName;

                return this.readOpSec.NextValue();
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

        internal float PerfCounterGetProcessWriteOpSec(string procName)
        {
            string cat = "Process";
            string counter = "IO Write Operations/sec";

            try
            {
                this.writeOpSec.CategoryName = cat;
                this.writeOpSec.CounterName = counter;
                this.writeOpSec.InstanceName = procName;

                return this.writeOpSec.NextValue();
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

        internal float PerfCounterGetProcessDataOpSec(string procName)
        {
            string cat = "Process";
            string counter = "IO Data Operations/sec";

            try
            {
                this.dataOpSec.CategoryName = cat;
                this.dataOpSec.CounterName = counter;
                this.dataOpSec.InstanceName = procName;

                return this.dataOpSec.NextValue();
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

        internal float PerfCounterGetProcessReadBytesSec(string procName)
        {
            string cat = "Process";
            string counter = "IO Read Bytes/sec";

            try
            {
                this.readBytesSec.CategoryName = cat;
                this.readBytesSec.CounterName = counter;
                this.readBytesSec.InstanceName = procName;

                return this.readBytesSec.NextValue();
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

        internal float PerfCounterGetProcessWriteByteSec(string procName)
        {
            string cat = "Process";
            string counter = "IO Write Bytes/sec";

            try
            {
                this.writeByteSec.CategoryName = cat;
                this.writeByteSec.CounterName = counter;
                this.writeByteSec.InstanceName = procName;

                return this.writeByteSec.NextValue();
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
        internal float PerfCounterGetProcessDataBytesSec(string procName)
        {
            string cat = "Process";
            string counter = "IO Data Bytes/sec";

            try
            {
                this.dataBytesSec.CategoryName = cat;
                this.dataBytesSec.CounterName = counter;
                this.dataBytesSec.InstanceName = procName;

                return this.dataBytesSec.NextValue();
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

                if (this.memProcessPrivateWorkingSetCounter != null)
                {
                    this.memProcessPrivateWorkingSetCounter.Dispose();
                    this.memProcessPrivateWorkingSetCounter = null;
                }

                if (this.readOpSec != null)
                {
                    this.readOpSec.Dispose();
                    this.readOpSec = null;
                }

                if (this.writeOpSec != null)
                {
                    this.writeOpSec.Dispose();
                    this.writeOpSec = null;
                }

                if (this.dataOpSec != null)
                {
                    this.dataOpSec.Dispose();
                    this.dataOpSec = null;
                }

                if (this.readBytesSec != null)
                {
                    this.readBytesSec.Dispose();
                    this.readBytesSec = null;
                }

                if (this.writeByteSec != null)
                {
                    this.writeByteSec.Dispose();
                    this.writeByteSec = null;
                }

                if (this.dataBytesSec != null)
                {
                    this.dataBytesSec.Dispose();
                    this.dataBytesSec = null;
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
