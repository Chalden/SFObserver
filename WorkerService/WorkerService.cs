using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace WorkerService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    public interface IWorkerService : IService
    {
        Task SubmitHeartbeat(Heartbeat heartbeat);
    }

    internal sealed class WorkerService : StatelessService, IWorkerService
    {
        private readonly TimeSpan TimeInterval = TimeSpan.FromSeconds(10);
        private readonly IServiceProxyFactory _heartbeatServiceProxyFactory;
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
            return this.CreateServiceRemotingInstanceListeners();
        }

        public async Task SubmitHeartbeat(Heartbeat heartbeat)
        {
            var proxy = ServiceProxy.Create<IHeartbeatService>(new Uri("fabric:/Heartbeat/HeartbeatService"), new ServicePartitionKey(0));
            await proxy.AddHeartbeat(heartbeat);
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            int workerStatusLength = Enum.GetNames(typeof(WorkerStatus)).Length;
            string senderId = this.Context.CodePackageActivationContext.ApplicationName + this.Context.InstanceId + this.Context.PartitionId.ToString();
            Random random = new Random(1);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                WorkerStatus status = (WorkerStatus)random.Next(workerStatusLength);
                Heartbeat heartbeat = new Heartbeat(senderId, DateTime.UtcNow, status.ToString());
                await SubmitHeartbeat(heartbeat);
                await Task.Delay(TimeInterval, cancellationToken);
            }
        }
    }
}
