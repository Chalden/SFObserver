using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HeartbeatService.Contract;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace HeartbeatService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class HeartbeatService : StatefulService, IHeartbeatService
    {
        public HeartbeatService(StatefulServiceContext context)
            : base(context)
        { }

        private IReliableDictionary<string, Heartbeat> Heartbeats;

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        public async Task SubmitHeartbeatAsync(Heartbeat heartbeat)
        {
            this.Heartbeats = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Heartbeat>>("heartbeats");

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                await Heartbeats.SetAsync(tx, heartbeat.SenderId, heartbeat);

                await tx.CommitAsync();
            }
        }

        public async Task<List<Heartbeat>> GetAllHeartbeatsAsync(CancellationToken cancellationToken)
        {
            bool nextValue = true;
            List<Heartbeat> heartbeatsList = new List<Heartbeat>();

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                var enumerable  = await this.Heartbeats.CreateEnumerableAsync(tx);
                var enumerator = enumerable.GetAsyncEnumerator();
                while (nextValue)
                {
                    Heartbeat currentHeartbeat = enumerator.Current.Value;
                    heartbeatsList.Add(currentHeartbeat);
                    nextValue = await enumerator.MoveNextAsync(cancellationToken);
                }
            }
            return heartbeatsList;
        }
    }
}
