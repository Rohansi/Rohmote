using System;
using System.Threading.Tasks;
using SuperWebSocket;

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
                        var data = RpcMessage.Write(message);
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
                    var message = RpcMessage.Read(data);
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
