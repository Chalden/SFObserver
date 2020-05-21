using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace WorkerService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class WorkerService : StatelessService
    {
        private readonly TimeSpan TimeInterval = TimeSpan.FromSeconds(10);

        private enum WorkerStatus { Running, Idle };

        public WorkerService(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string senderId =  this.Context.CodePackageActivationContext.ApplicationName + this.Context.InstanceId + this.Context.PartitionId.ToString();
                string timestamp = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss");

                Random random = new Random();
                int workerStatusLength = Enum.GetNames(typeof(WorkerStatus)).Length;
                WorkerStatus status = (WorkerStatus) random.Next(workerStatusLength);
                
                ServiceEventSource.Current.ServiceMessage(this.Context, senderId + timestamp + status);
                
                await Task.Delay(TimeInterval, cancellationToken);
            }
        }
    }
}
