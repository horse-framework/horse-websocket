using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Horse.WebSocket.Models.Providers
{
    internal class PayloadFrame<TModel>
    {
        [JsonProperty("type")]
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonProperty("payload")]
        [JsonPropertyName("payload")]
        public TModel Payload { get; set; }
    }
}