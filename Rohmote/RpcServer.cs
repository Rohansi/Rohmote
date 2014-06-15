using System;
using System.Threading.Tasks;
using SuperWebSocket;
using Newtonsoft.Json;

namespace Rohmote
{
    public class RpcServer
    {
        private class Connection : WebSocketSession<Connection>
        {
            public RpcProcessor Processor;
        }

        private class Server : WebSocketServer<Connection>
        {

        }

        public RpcServer(string ip, int port, Action<RpcProcessor> initializer)
        {
            var server = new Server();
            server.Setup(ip, port);

            server.NewSessionConnected += connection =>
            {
                var rpc = new RpcProcessor();
                connection.Processor = rpc;

                rpc.Send = message =>
                {
                    try
                    {
                        var data = message.Type + JsonConvert.SerializeObject(message);
                        connection.Send(data);
                    }
                    catch (Exception e)
                    {
                        connection.Processor.DispatchError(e);
                    }
                };

                initializer(rpc);
            };

            server.NewMessageReceived += (connection, data) =>
            {
                try
                {
                    var type = data.Substring(0, 3);
                    var json = data.Substring(3);

                    IRpcMessage message;

                    switch (type)
                    {
                        case "req":
                            message = JsonConvert.DeserializeObject<RpcRequest>(json);
                            break;
                        case "res":
                            message = JsonConvert.DeserializeObject<RpcResponse>(json);
                            break;
                        default:
                            throw new NotSupportedException("Unsupported message type: " + type);
                    }

                    Task.Run(() => connection.Processor.ProcessMessage(message));
                }
                catch (Exception e)
                {
                    connection.Processor.DispatchError(e);
                }
            };

            server.Start();
        }
    }
}
