using System;
using System.Threading.Tasks;
using WebSocket4Net;

namespace Rohmote
{
    public class RpcClient : RpcProcessor
    {
        private WebSocket _client;

        public RpcClient(string ip, int port)
        {
            var uri = string.Format("ws://{0}:{1}/", ip, port);
            _client = new WebSocket(uri);

            _client.Opened += (sender, args) =>
                Task.Run(() => DispatchConnected());

            _client.Closed += (sender, args) =>
                Task.Run(() => SafeCall(DispatchDisconnected));

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

        public override void Dispose()
        {
            if (_client != null)
                _client.Close();

            base.Dispose();
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
