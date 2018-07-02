using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ServiceFabric.Services.Remoting.V2;

namespace SharedProject
{
    public static class ServiceRemotingRequestMessageExtensions
    {
        private const string RequestDataHeaderKey = "RequestDataHeaderKey";
        private const string ColerationIdHeaderKey = "ColerationIdHeaderKey";
        

        public static Guid GetColerationId(this IServiceRemotingRequestMessage requestMessage)
        {
            var headers = requestMessage.GetHeader();
            if (headers.TryGetHeaderValue(ServiceRemotingRequestMessageExtensions.ColerationIdHeaderKey, out var colerationIdValue))
            {
                return new Guid(colerationIdValue);
            }
            else
                return Guid.Empty;
        }

        public static RequestData GetRequestData(this IServiceRemotingRequestMessage requestMessage)
        {
            var headers = requestMessage.GetHeader();
            if (headers.TryGetHeaderValue(ServiceRemotingRequestMessageExtensions.RequestDataHeaderKey, out var requestDataValue))
            {
                return RequestData.Deserialize(requestDataValue);
            }
            else
                return null;
        }


        public static void SetRequestData(this IServiceRemotingRequestMessage requestMessage, RequestData requestData)
        {
            var headers = requestMessage.GetHeader();
            headers.AddHeader(ServiceRemotingRequestMessageExtensions.RequestDataHeaderKey, requestData.Serialize());
        }

        public static void SetColerationId(this IServiceRemotingRequestMessage requestMessage, Guid collerationId)
        {
            var headers = requestMessage.GetHeader();
            headers.AddHeader(ServiceRemotingRequestMessageExtensions.ColerationIdHeaderKey, collerationId.ToByteArray());
        }

    }
}
