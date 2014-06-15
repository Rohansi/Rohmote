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
                    connected();
            };

            client.Closed += (sender, args) =>
            {
                var disconnected = Disconnected;
                if (disconnected != null)
                    disconnected();
            };

            Send = message =>
            {
                try
                {
                    var data = RpcMessage.Write(message);
                    client.Send(data);
                }
                catch (Exception e)
                {
                    DispatchError(e);
                }
            };

            client.MessageReceived += (sender, args) =>
            {
                try
                {
                    var data = args.Message;
                    var message = RpcMessage.Read(data);
                    Task.Run(() => ProcessMessage(message));
                }
                catch (Exception e)
                {
                    DispatchError(e);
                }
            };

            client.Open();
        }
    }
}
