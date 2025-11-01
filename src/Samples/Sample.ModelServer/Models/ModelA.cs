using System.Text.Json.Serialization;
using Horse.WebSocket.Protocol;

namespace Sample.ModelServer.Models
{
    [TextMessageType("console-request")]
    public class ModelA
    {
        [JsonPropertyName("v")]
        public string Value { get; set; }
    }
}