using System.Text.Json.Serialization;

namespace Horse.WebSocket.Models.Providers
{
    internal class PayloadResolve
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}