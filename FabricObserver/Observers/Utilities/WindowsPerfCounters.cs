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
        private PerformanceCounter memProcessPrivateWorkingSetCounter;
        private List<PerformanceCounter> countersList;

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
                
                this.countersList = new List<PerformanceCounter>();
                this.countersList.Add(new PerformanceCounter() 
                    {CategoryName = "Process", CounterName = "IO Read Operations/sec"});
                this.countersList.Add(new PerformanceCounter()
                    { CategoryName = "Process", CounterName = "IO Write Operations/sec"});
                this.countersList.Add(new PerformanceCounter()
                    { CategoryName = "Process", CounterName = "IO Data Operations/sec"});
                this.countersList.Add(new PerformanceCounter()
                    { CategoryName = "Process", CounterName = "IO Read Bytes/sec"});
                this.countersList.Add(new PerformanceCounter()
                    { CategoryName = "Process", CounterName = "IO Write Bytes/sec"});
                this.countersList.Add(new PerformanceCounter()
                    { CategoryName = "Process", CounterName = "IO Data Bytes/sec"});
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

        internal List<float> PerfCounterGetProcessIO(string procName)
        {
            List<float> results = new List<float>();

            foreach (PerformanceCounter perfCounter in this.countersList)
            {
                string category = perfCounter.CategoryName;
                string counter = perfCounter.CounterName;

                try
                {
                    perfCounter.InstanceName = procName;

                    results.Add(perfCounter.NextValue());
                }
                catch (Exception e)
                {
                    if (e is ArgumentNullException || e is PlatformNotSupportedException
                                                   || e is System.ComponentModel.Win32Exception ||
                                                   e is UnauthorizedAccessException)
                    {
                        this.Logger.LogError($"{category} {counter} PerfCounter handled error: " + e);

                        // Don't throw.
                        results.Add(0F);
                    }

                    this.Logger.LogError($"{category} {counter} PerfCounter unhandled error: " + e);

                    throw;
                }
            }
            return results;
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

                if (this.countersList != null)
                {
                    foreach (PerformanceCounter perfCounter in this.countersList)
                    {
                        perfCounter.Dispose();
                    }
                    this.countersList = null;
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
