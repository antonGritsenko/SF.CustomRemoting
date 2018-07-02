using System;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Runtime;

namespace SharedProject
{
    /// <summary>
    /// Custom message dispatcher. 
    /// Used to get data from message headers.
    /// Must be used in <see cref="ExtendedFabricTransportServiceRemotingProviderAttribute.CreateServiceRemotingListenerV2(ServiceContext, IService)"/>
    /// </summary>
    public class ExtendedServiceRemotingDispatcher : ServiceRemotingMessageDispatcher
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="serviceContext"></param>
        /// <param name="service"></param>
        public ExtendedServiceRemotingDispatcher(ServiceContext serviceContext, IService service) :
            base(serviceContext, service)
        {
        }

        /// <summary>
        /// Handle request and getting information from it, then put it to <see cref="System.Runtime.Remoting.Messaging.CallContext"/>
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        /// <seealso cref="ServiceRemotingRequestMessageExtensions"/>
        public override Task<IServiceRemotingResponseMessage> HandleRequestResponseAsync(IServiceRemotingRequestContext requestContext, IServiceRemotingRequestMessage requestMessage)
        {
            /// put info to <see cref="System.Runtime.Remoting.Messaging.CallContext"/> using helper methods from <see cref="ServiceRemotingRequestMessageExtensions"/>
            // we can use CallContext directly, because in any case this method can't be unit-tested
            var correlationId = requestMessage.GetColerationId();
            var requestData = requestMessage.GetRequestData();
            // execute base handler in context of the service to send data to CallContext
            return ServiceRequestContext.RunInRequestContext(async () =>
                    await base.HandleRequestResponseAsync(
                        requestContext,
                        requestMessage),
                correlationId, requestData);
        }
    }
}
