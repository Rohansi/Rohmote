using System;
using System.Threading.Tasks;
using SuperWebSocket;

namespace Rohmote
{
    public class RpcServer : IDisposable
    {
        private class Connection : WebSocketSession<Connection>
        {
            public RpcProcessor Processor;
        }

        private class Server : WebSocketServer<Connection>
        {

        }

        private Server _server;

        public RpcServer(string ip, int port, Action<RpcProcessor> initializer)
        {
            _server = new Server();
            _server.Setup(ip, port);

            _server.NewSessionConnected += connection =>
            {
                var rpc = new RpcProcessor();
                connection.Processor = rpc;

                rpc.Send = message => SafeCall(connection, () =>
                {
                    var data = RpcMessage.Write(message);
                    connection.Send(data);
                });

                initializer(rpc);
            };

            _server.NewMessageReceived += (connection, data) => SafeCall(connection, () =>
            {
                var message = RpcMessage.Read(data);
                Task.Run(() => connection.Processor.ProcessMessage(message));
            });

            _server.SessionClosed += (connection, value) =>
                SafeCall(connection, () => connection.Processor.DispatchDisconnected());

            _server.Start();
        }

        public void Dispose()
        {
            _server.Stop();

            foreach (var connection in _server.GetAllSessions())
            {
                connection.Processor.Dispose();
                connection.CloseWithHandshake("Closing");
            }

            _server.Dispose();
        }

        private static void SafeCall(Connection connection, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                connection.Processor.DispatchError(e);
            }
        }
    }
}
