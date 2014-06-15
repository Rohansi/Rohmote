using System;
using System.Threading.Tasks;
using WebSocket4Net;

namespace Rohmote
{
    public class RpcClient : RpcProcessor
    {
        public event Action Connected;
        public event Action Disconnected;

        public RpcClient(string ip, int port)
        {
            var uri = string.Format("ws://{0}:{1}/", ip, port);
            var client = new WebSocket(uri);

            client.Opened += (sender, args) =>
            {
                var connected = Connected;
                if (connected != null)
                    Task.Run(() => SafeCall(connected));
            };

            client.Closed += (sender, args) =>
            {
                var disconnected = Disconnected;
                if (disconnected != null)
                    Task.Run(() => SafeCall(disconnected));
            };

            Send = message => SafeCall(() =>
            {
                var data = RpcMessage.Write(message);
                client.Send(data);
            });

            client.MessageReceived += (sender, args) => SafeCall(() =>
            {
                var data = args.Message;
                var message = RpcMessage.Read(data);
                Task.Run(() => ProcessMessage(message));
            });

            Task.Run(() => SafeCall(client.Open));
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
