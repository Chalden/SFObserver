using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace TestService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class TestService : StatelessService
    {
        public TestService(StatelessServiceContext context)
            : base(context)
        { }

        Random rand = new Random();

        //List of threads
        List<Thread> threadsList = new List<Thread>();

        // List of byte arrays
        List<byte[]> byteArraysList = new List<byte[]>();

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
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            while (true)
            {
                //threadsList.Add(new Thread(new ThreadStart(KillCore)));
                byteArraysList.Add(new byte[1000000]);
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            
            //return Task.CompletedTask;
        }
        public void KillCore()
        {
            long num = 0;
            while (true)
            {
                num += rand.Next(100, 1000);
                if (num > 1000000) { num = 0; }
            }
        }
    }
}
