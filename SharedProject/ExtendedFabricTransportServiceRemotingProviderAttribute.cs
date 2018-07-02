using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Fabric;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SharedProject
{
    /// <summary>
    /// Used to add ability to transfer data from service to service using Message Headers in Remoting.
    /// Must be defined on assembly level
    /// Adds <see cref="ExtendedServiceRemotingDispatcher"/> to <see cref="FabricTransportServiceRemotingListener"/> that used in <see cref="ServiceRemotingExtensions.CreateServiceRemotingInstanceListeners"/>
    /// This attribute support only V2 remoting
    /// </summary>
    /// <remarks>
    /// See more here: https://stackoverflow.com/questions/41629755/passing-user-and-auditing-information-in-calls-to-reliable-services-in-service-f
    /// </remarks>
    public class ExtendedFabricTransportServiceRemotingProviderAttribute : FabricTransportServiceRemotingProviderAttribute
    {
        /// <summary>
        /// Create custom listener (service side) for Remoting V2 and add <see cref="ExtendedServiceRemotingDispatcher"/> to pipeline
        /// </summary>
        /// <param name="serviceContext"></param>
        /// <param name="serviceImplementation"></param>
        /// <returns></returns>
        public override IServiceRemotingListener CreateServiceRemotingListenerV2(ServiceContext serviceContext, IService serviceImplementation)
        {
            // create our own dispatcher, this will handle adding info to headers
            var messageHandler = new ExtendedServiceRemotingDispatcher(serviceContext, serviceImplementation);

            // note: attributes names remoting for V1 and V2 are NOT the same
            var listner = new FabricTransportServiceRemotingListener(serviceContext: serviceContext, serviceRemotingMessageHandler: messageHandler);

            return (IServiceRemotingListener)listner;
        }

        /// <summary>
        /// Create custom client using <see cref="ExtendedFabricTransportServiceRemotingClientFactory" />
        /// </summary>
        /// <param name="callbackMessageHandler"></param>
        /// <returns></returns>
        public override IServiceRemotingClientFactory CreateServiceRemotingClientFactoryV2(IServiceRemotingCallbackMessageHandler callbackMessageHandler)
        {
            // debug code to get all registered even sources from .net framework (see https://referencesource.microsoft.com/mscorlib/system/diagnostics/eventing/eventsource.cs.html#5dcdbf0a2aacfd4c)
            //var p = typeof(EventListener).GetField("s_EventSources", BindingFlags.Static | BindingFlags.NonPublic);
            //var r = p.GetValue(null);
            // send default implementation as attribute
            return new ExtendedFabricTransportServiceRemotingClientFactory(base.CreateServiceRemotingClientFactoryV2(callbackMessageHandler));
        }

    }
}
