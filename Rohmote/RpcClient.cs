using System;
using System.Threading.Tasks;
using WebSocket4Net;

namespace Rohmote
{
    public class RpcClient : RpcProcessor
    {
        public event Action Connected;
        public event Action Disconnected;

        private WebSocket _client;

        public RpcClient(string ip, int port)
        {
            var uri = string.Format("ws://{0}:{1}/", ip, port);
            _client = new WebSocket(uri);

            _client.Opened += (sender, args) =>
            {
                var connected = Connected;
                if (connected != null)
                    Task.Run(() => SafeCall(connected));
            };

            _client.Closed += (sender, args) =>
            {
                var disconnected = Disconnected;
                if (disconnected != null)
                    Task.Run(() => SafeCall(disconnected));
            };

            Send = message => SafeCall(() =>
            {
                var data = RpcMessage.Write(message);
                _client.Send(data);
            });

            _client.MessageReceived += (sender, args) => SafeCall(() =>
            {
                var data = args.Message;
                var message = RpcMessage.Read(data);
                Task.Run(() => ProcessMessage(message));
            });

            Task.Run(() => SafeCall(_client.Open));
        }

        public void Disconnect()
        {
            SafeCall(() => _client.Close("Disconnecting"));
        }

        private void SafeCall(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                DispatchError(e);
            }
        }
    }
}
