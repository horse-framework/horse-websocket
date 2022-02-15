using System.Text.Json.Serialization;
using Horse.WebSocket.Protocol;
using Newtonsoft.Json;

namespace Sample.ModelServer.Models
{
    [ModelType("console-request")]
    public class ModelA
    {
        [JsonProperty("v")]
        [JsonPropertyName("v")]
        public string Value { get; set; }
    }
}