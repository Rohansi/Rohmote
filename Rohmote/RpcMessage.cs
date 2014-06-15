using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rohmote
{
    internal enum RpcMessageType
    {
        Request, Response
    }

    internal interface IRpcMessage
    {
        RpcMessageType Type { get; }
    }

    internal class RpcRequest : IRpcMessage
    {
        [JsonIgnore]
        public RpcMessageType Type { get { return RpcMessageType.Request; } }

        [JsonProperty("id")]
        public string Id;

        [JsonProperty("method")]
        public string Method;

        [JsonProperty("params")]
        public JToken[] Parameters;
    }

    internal class RpcResponse : IRpcMessage
    {
        [JsonIgnore]
        public RpcMessageType Type { get { return RpcMessageType.Response; } }

        [JsonProperty("id")]
        public string Id;

        [JsonProperty("result")]
        public JToken Result;

        [JsonProperty("error")]
        public string Error;
    }

    internal static class RpcMessage
    {
        public static IRpcMessage Read(string data)
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
                    throw new NotSupportedException(string.Format("Unsupported message type: '{0}'", type));
            }

            return message;
        }

        public static string Write(IRpcMessage message)
        {
            string type;
            switch (message.Type)
            {
                case RpcMessageType.Request:
                    type = "req";
                    break;
                case RpcMessageType.Response:
                    type = "res";
                    break;
                default:
                    throw new NotSupportedException(string.Format("Unsupported message type '{0}'", message.Type));
            }

            var data = type + JsonConvert.SerializeObject(message);
            return data;
        }
    }
}
