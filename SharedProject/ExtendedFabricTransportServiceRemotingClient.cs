using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharedProject
{
    /// <summary>
    /// This custom remoting client is just extension of the standard client from Service Fabric
    /// </summary>
    /// <remarks>
    /// Designed to use in <see cref="ExtendedFabricTransportServiceRemotingClientFactory"/> and should not be used directly
    /// </remarks>
    public class ExtendedFabricTransportServiceRemotingClient : IServiceRemotingClient, ICommunicationClient
    {
        // save standard client 
        private readonly IServiceRemotingClient innerClient;

        /// <summary>
        /// Standard Service Fabric client instance
        /// </summary>
        internal IServiceRemotingClient BaseClient { get { return this.innerClient; } }

        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="innerClient">Standard Service Fabric client</param>
        public ExtendedFabricTransportServiceRemotingClient(IServiceRemotingClient innerClient)
        {
            this.innerClient = innerClient ?? throw new ArgumentNullException(nameof(innerClient));
        }

        /// <summary>
        /// Clean up 
        /// </summary>
        ~ExtendedFabricTransportServiceRemotingClient()
        {
            if (this.innerClient == null) return;
            var disposable = this.innerClient as IDisposable;
            disposable?.Dispose();
        }

        /// <summary>
        /// Implemented using standard Service Fabric client, no headers injection
        /// </summary>
        /// <param name="requestMessage"></param>
        public void SendOneWay(IServiceRemotingRequestMessage requestMessage)
        {
            this.innerClient.SendOneWay(requestMessage);
        }

        /// <summary>
        /// Inject data to headers and transfer to service using standard Service Fabric client
        /// </summary>
        /// <param name="requestRequestMessage"></param>
        /// <returns></returns>
        /// <seealso cref="ServiceRemotingRequestMessageExtensions"/>
        public Task<IServiceRemotingResponseMessage> RequestResponseAsync(IServiceRemotingRequestMessage requestRequestMessage)
        {
            /// could be null if call executed outside of <see cref="ServiceRequestContext.RunInRequestContext(Func{Task}, Guid, RequestData)"/> or <see cref="ServiceRequestContext.RunInRequestContext{TResult}(Func{Task{TResult}}, Guid, RequestData)"/> 
            if (ServiceRequestContext.Current != null)
            {
                // put data to headers using extensions
                requestRequestMessage.SetRequestData(ServiceRequestContext.Current.RequestData);
                requestRequestMessage.SetColerationId(ServiceRequestContext.Current.CorrelationId);
            }
            /// Execute standard RequestResponseAsync
            return this.innerClient.RequestResponseAsync(requestRequestMessage);
        }

        /// <summary>
        /// Implemented using standard Service Fabric client
        /// </summary>
        public ResolvedServicePartition ResolvedServicePartition
        {
            get { return this.innerClient.ResolvedServicePartition; }
            set { this.innerClient.ResolvedServicePartition = value; }
        }

        /// <summary>
        /// Implemented using standard Service Fabric client
        /// </summary>
        public string ListenerName
        {
            get { return this.innerClient.ListenerName; }
            set { this.innerClient.ListenerName = value; }
        }

        /// <summary>
        /// Implemented using standard Service Fabric client
        /// </summary>
        public ResolvedServiceEndpoint Endpoint
        {
            get { return this.innerClient.Endpoint; }
            set { this.innerClient.Endpoint = value; }
        }
    }
}
