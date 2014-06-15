using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Rohmote
{
    public class RpcProcessor : IDisposable
    {
        public event Action Connected;
        public event Action Disconnected;
        public event Action<Exception> Error;

        public bool DetailedErrorMessages { get; set; }
        public TimeSpan CallTimeOut { get; set; }
        public object Tag { get; set; }

        internal delegate Task<JToken> RpcHandler(RpcProcessor sender, JToken[] parameters);
        internal Action<IRpcMessage> Send;

        private ConcurrentDictionary<string, RpcHandler> _handlers;
        private ConcurrentDictionary<string, TaskCompletionSource<JToken>> _requests;

        internal RpcProcessor()
        {
            _handlers = new ConcurrentDictionary<string, RpcHandler>();
            _requests = new ConcurrentDictionary<string, TaskCompletionSource<JToken>>();

            DetailedErrorMessages = false;
            CallTimeOut = TimeSpan.FromSeconds(5);
        }

        public virtual void Dispose()
        {
            Send = null;
            _handlers.Clear();
            _requests.Clear();
        }

        internal void Register(string method, RpcHandler handler)
        {
            if (!_handlers.TryAdd(method, handler))
                throw new Exception("Duplicate RPC handler");
        }

        internal async Task<JToken> Invoke(string method, JToken[] parameters)
        {
            var id = Guid.NewGuid().ToString();

            try
            {
                var completion = new TaskCompletionSource<JToken>();

                if (!_requests.TryAdd(id, completion))
                    throw new Exception("Duplicate request id");

                Send(new RpcRequest
                {
                    Id = id,
                    Method = method,
                    Parameters = parameters
                });

                var timeout = Task.Delay(CallTimeOut);
                var completed = await Task.WhenAny(completion.Task, timeout);

                if (completed == timeout)
                    throw new Exception("Request timed out");

                return completion.Task.Result;
            }
            finally
            {
                TaskCompletionSource<JToken> removed;
                _requests.TryRemove(id, out removed);
            }
        }

        internal async Task ProcessMessage(IRpcMessage message)
        {
            try
            {
                switch (message.Type)
                {
                    case RpcMessageType.Request:
                        var request = (RpcRequest)message;

                        try
                        {
                            RpcHandler handler;
                            if (!_handlers.TryGetValue(request.Method, out handler))
                                throw new Exception(string.Format("Unknown method '{0}'", request.Method));

                            var result = await handler(this, request.Parameters);

                            Send(new RpcResponse
                            {
                                Id = request.Id,
                                Result = result,
                                Error = null
                            });
                        }
                        catch (Exception e)
                        {
                            Send(new RpcResponse
                            {
                                Id = request.Id,
                                Result = null,
                                Error = ErrorString(e)
                            });
                        }
                        break;

                    case RpcMessageType.Response:
                        var response = (RpcResponse)message;

                        TaskCompletionSource<JToken> completion;
                        if (!_requests.TryGetValue(response.Id, out completion))
                            throw new Exception("Unknown request id");

                        if (response.Error != null)
                        {
                            completion.SetException(new RpcException(response.Error));
                            return;
                        }

                        completion.SetResult(response.Result);
                        break;

                    default:
                        throw new NotSupportedException(string.Format("Unsupported message type '{0}'", message.Type));
                }
            }
            catch (Exception e)
            {
                DispatchError(e);
            }
        }

        internal void DispatchConnected()
        {
            var connected = Connected;
            if (connected != null)
                connected();
        }

        internal void DispatchDisconnected()
        {
            var disconnected = Disconnected;
            if (disconnected != null)
                disconnected();
        }

        internal void DispatchError(Exception e)
        {
            var error = Error;
            if (error != null)
                error(e);
        }

        private string ErrorString(Exception e)
        {
            e = UnwrapAggregate(e);
            if (DetailedErrorMessages)
                return e.ToString();
            return e.Message;
        }

        private static Exception UnwrapAggregate(Exception e)
        {
            var a = e as AggregateException;
            if (a == null)
                return e;

            a = a.Flatten();

            if (a.InnerExceptions.Count == 1)
                return a.InnerExceptions[0];

            return a;
        }
    }
}
