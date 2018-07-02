using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SharedProject
{
    /// <summary>
    /// Represents data shared between all requests. This data transfered from service to service using <see cref="ServiceRequestContext"/>
    /// </summary>
    /// <remarks>
    /// You must add in this class information that can be helpful during request in any service, like language, user id and so on
    /// Each and every property in this class must support serialization by BinaryFormatter
    /// </remarks>
    /// <seealso cref="ExtendedFabricTransportServiceRemotingProviderAttribute"/>
    /// <seealso cref="System.Runtime.Remoting.Messaging.CallContext"/>
    [Serializable]
    public class RequestData
    {
        /// <summary>
        /// Default settings, please use it if RequestData empty or some of parameters are empty
        /// </summary>
        public static RequestData Default { get { return new RequestData { CultureCode = "en", UILanguage = "en", UserId = "" }; } }

        /// <summary>
        /// Default language for the tenant. Could override by settings
        /// </summary>
        public string DefaultUILanguage { get; set; } = "en";

        /// <summary>
        /// Language used for localization (display strings in UI and API)
        /// </summary>
        public string UILanguage { get; set; }
        /// <summary>
        /// Culture used for globalization (dateime, number format etc)
        /// </summary>
        public string CultureCode { get; set; }
        /// <summary>
        /// User name (for a future use)
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Serialize object to store in <see cref="System.Runtime.Remoting.Messaging.CallContext"/>
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            var formatter = new BinaryFormatter();
            byte[] content;
            using (var ms = new MemoryStream())
            {
                // compress data
                using (var ds = new DeflateStream(ms, CompressionMode.Compress, true))
                {
                    formatter.Serialize(ds, this);
                }
                ms.Position = 0;
                content = ms.GetBuffer();
            }

            return content;
        }

        /// <summary>
        /// Create object from byte array
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static RequestData Deserialize(byte[] data)
        {
            var formatter = new BinaryFormatter();
            using (var ms = new MemoryStream(data))
            {
                using (var ds = new DeflateStream(ms, CompressionMode.Decompress, true))
                {
                   return (RequestData)formatter.Deserialize(ds);
                }
            }
        }
    }
}
