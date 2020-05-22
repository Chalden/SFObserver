using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeartbeatService
{
    class HttpCommunicationListener : ICommunicationListener
    {
        public void Abort()
        {
            throw new NotImplementedException();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            EndpointResourceDescription endpoint =
                serviceContext.CodePackageActivationContext.GetEndpoint("WebEndpoint");

            string uriPrefix = $"{endpoint.Protocol}://+:{endpoint.Port}/myapp/";

            this.httpListener = new HttpListener();
            this.httpListener.Prefixes.Add(uriPrefix);
            this.httpListener.Start();

            string publishUri = uriPrefix.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
            return Task.FromResult(publishUri);
        }
    }
}
