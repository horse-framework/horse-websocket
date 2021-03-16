using System.Text.Json.Serialization;
using Horse.WebSocket.Models;
using Newtonsoft.Json;

namespace Sample.ModelServer.Models
{
    [ModelType("model-a")]
    public class ModelA
    {
        [JsonProperty("v")]
        [JsonPropertyName("v")]
        public string Value { get; set; }
    }
}