using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Runtime;
using WorkerService;

namespace HeartbeatService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class HeartbeatService : StatefulService
    {
        public HeartbeatService(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                IWorkerService workerClient = ServiceProxy.Create<IWorkerService>(new Uri("fabric:/Worker/WorkerService"), new ServicePartitionKey(0));
                string message = await workerClient.SendHeartbeat(cancellationToken);
            }
        }
    }
}
