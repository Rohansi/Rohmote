using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rohmote
{
    internal interface IRpcMessage
    {
        string Type { get; }
    }

    internal class RpcRequest : IRpcMessage
    {
        [JsonIgnore]
        public string Type { get { return "req"; } }

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
        public string Type { get { return "res"; } }

        [JsonProperty("id")]
        public string Id;

        [JsonProperty("result")]
        public JToken Result;

        [JsonProperty("error")]
        public string Error;
    }
}
