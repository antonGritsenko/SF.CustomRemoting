using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedProject
{
    /// <summary>
    /// Interface must be implemented by any class used to store data
    /// Main goal of this interface is to simplify unit testing
    /// </summary>
    public interface IServiceRequestContext
    {
        Guid CorrelationId { get; }
        RequestData RequestData { get; }

        /// <summary>
        /// Get request current request data or default
        /// </summary>
        /// <returns></returns>
        RequestData GetRequestDataOrDefault();
    }

    /// <summary>
    /// Create and handle remoting call context
    /// This class instance will be transfered between calls using Service Fabric Remoting
    /// </summary>
    /// <remarks>
    /// </remarks>
    public sealed class ServiceRequestContext : IServiceRequestContext
    {
        private static readonly string ContextKey = Guid.NewGuid().ToString();

        /// <summary>
        /// Create new context with correlation id and request data
        /// </summary>
        /// <param name="correlationId"></param>
        /// <param name="requestData"></param>
        public ServiceRequestContext(Guid correlationId, RequestData requestData)
        {
            this.CorrelationId = correlationId;
            this.RequestData = requestData;
        }

        /// <summary>
        /// Unique of the call. This ID init on API level and then transfered to all other services
        /// </summary>
        public Guid CorrelationId { get; private set; }

        /// <summary>
        /// Any request data
        /// </summary>
        public RequestData RequestData { get; private set; }

        /// <summary>
        /// Return current context of the remoting call
        /// Based on Service Fabric Remoting
        /// </summary>
        public static ServiceRequestContext Current
        {
            get { return (ServiceRequestContext)System.Runtime.Remoting.Messaging.CallContext.LogicalGetData(ContextKey); }
            internal set
            {
                if (value == null)
                {
                    System.Runtime.Remoting.Messaging.CallContext.FreeNamedDataSlot(ContextKey);
                }
                else
                {
                    System.Runtime.Remoting.Messaging.CallContext.LogicalSetData(ContextKey, value);
                }
            }
        }

        /// <summary>
        /// This is wrap method to call any async methods in the context
        /// You can use it inside any method which use remoting or in middleware to wrap all methods
        /// </summary>
        /// <param name="action"></param>
        /// <param name="correlationId"></param>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public static Task RunInRequestContext(Func<Task> action, Guid correlationId, RequestData requestData)
        {
            Task<Task> task = null;
            task = new Task<Task>(async () =>
            {
                ServiceRequestContext.Current = new ServiceRequestContext(correlationId, requestData);
                try
                {
                    await action();
                }
                finally
                {
                    ServiceRequestContext.Current = null;
                }
            });
            task.Start();
            return task.Unwrap();
        }

        /// <summary>
        /// This is wrap method to call any async methods in the context
        /// You can use it inside any method which use remoting or in middleware to wrap all methods
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="correlationId"></param>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public static Task<TResult> RunInRequestContext<TResult>(Func<Task<TResult>> action, Guid correlationId, RequestData requestData)
        {
            Task<Task<TResult>> task = null;
            task = new Task<Task<TResult>>(async () =>
            {
                ServiceRequestContext.Current = new ServiceRequestContext(correlationId, requestData);
                try
                {
                    return await action();
                }
                finally
                {
                    ServiceRequestContext.Current = null;
                }
            });
            task.Start();
            return task.Unwrap<TResult>();
        }

        /// <summary>
        /// Created to implement interface, should never be used in production code
        /// </summary>
        /// <returns></returns>
        public RequestData GetRequestDataOrDefault()
        {
            throw new NotImplementedException();
        }
    }
}
