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
    /// Custom client fabric for Service Fabric Remoting V2
    /// Used to return <see cref="ExtendedFabricTransportServiceRemotingClient"/>
    /// </summary>
    /// <remarks>
    /// To avoid re-implementation, this client fabric just get default client fabric (it transferred as ctor parameter) and use it to emulate standard behavior
    /// </remarks>
    public class ExtendedFabricTransportServiceRemotingClientFactory : IServiceRemotingClientFactory, ICommunicationClientFactory<IServiceRemotingClient>
    {
        // this is default implementation of the Fabric
        IServiceRemotingClientFactory innerClientFactory;

        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="innerClientFactory">Default implementation of the client factory</param>
        public ExtendedFabricTransportServiceRemotingClientFactory(IServiceRemotingClientFactory innerClientFactory)
        {
            // save default implementation for future use
            this.innerClientFactory = innerClientFactory ?? throw new ArgumentNullException(nameof(innerClientFactory));
            // we must react on event in same way as default fabric
            this.innerClientFactory.ClientConnected += InnerClientFactory_ClientConnected;
            this.innerClientFactory.ClientDisconnected += InnerClientFactory_ClientDisconnected;
        }

        /// <summary>
        /// Re-throw ClientDisconnected from standard client fabric event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InnerClientFactory_ClientDisconnected(object sender, CommunicationClientEventArgs<IServiceRemotingClient> e)
        {
            EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> clientDisconnected = this.ClientDisconnected;
            if (clientDisconnected == null) return;
            clientDisconnected((object)this, new CommunicationClientEventArgs<IServiceRemotingClient>()
            {
                Client = e.Client
            });
        }

        /// <summary>
        /// Re-throw ClientConnected from standard client fabric event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InnerClientFactory_ClientConnected(object sender, CommunicationClientEventArgs<IServiceRemotingClient> e)
        {
            EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> clientConnected = this.ClientConnected;
            if (clientConnected == null) return;
            clientConnected((object)this, new CommunicationClientEventArgs<IServiceRemotingClient>()
            {
                Client = e.Client
            });

        }

        #region Implementation of the ICommunicationClientFactory<IServiceRemotingClient>
        // this is implementation of the ICommunicationClientFactory<IServiceRemotingClient>
        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientConnected;
        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientDisconnected;

        /// <summary>
        /// Return <see cref="ExtendedFabricTransportServiceRemotingClient"/> client
        /// </summary>
        /// <param name="serviceUri"></param>
        /// <param name="partitionKey"></param>
        /// <param name="targetReplicaSelector"></param>
        /// <param name="listenerName"></param>
        /// <param name="retrySettings"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IServiceRemotingClient> GetClientAsync(Uri serviceUri, ServicePartitionKey partitionKey, TargetReplicaSelector targetReplicaSelector, string listenerName, OperationRetrySettings retrySettings, CancellationToken cancellationToken)
        {
            var baseClient = await this.innerClientFactory.GetClientAsync(serviceUri, partitionKey, targetReplicaSelector, listenerName, retrySettings, cancellationToken);
            return new ExtendedFabricTransportServiceRemotingClient(baseClient);
        }

        /// <summary>
        /// Return <see cref="ExtendedFabricTransportServiceRemotingClient"/> client
        /// </summary>
        /// <param name="previousRsp"></param>
        /// <param name="targetReplicaSelector"></param>
        /// <param name="listenerName"></param>
        /// <param name="retrySettings"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IServiceRemotingClient> GetClientAsync(ResolvedServicePartition previousRsp, TargetReplicaSelector targetReplicaSelector, string listenerName, OperationRetrySettings retrySettings, CancellationToken cancellationToken)
        {
            var baseClient = await this.innerClientFactory.GetClientAsync(previousRsp, targetReplicaSelector, listenerName, retrySettings, cancellationToken);
            return new ExtendedFabricTransportServiceRemotingClient(baseClient);
        }

        /// <summary>
        /// Handle exception using standard fabric client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="exceptionInformation"></param>
        /// <param name="retrySettings"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<OperationRetryControl> ReportOperationExceptionAsync(IServiceRemotingClient client, ExceptionInformation exceptionInformation, OperationRetrySettings retrySettings, CancellationToken cancellationToken)
        {
            return await this.innerClientFactory.ReportOperationExceptionAsync((client as ExtendedFabricTransportServiceRemotingClient).BaseClient, exceptionInformation, retrySettings, cancellationToken);
        }
        #endregion

        #region Implementation of the IServiceRemotingClientFactory
        // this is implementation of the IServiceRemotingClientFactory

        /// <summary>
        /// Get Remoting Message Body Factory from standard client fabric
        /// </summary>
        /// <returns></returns>
        public IServiceRemotingMessageBodyFactory GetRemotingMessageBodyFactory()
        {
            return this.innerClientFactory.GetRemotingMessageBodyFactory();
        }
        #endregion
    }
}
