using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Twino.SerializableModel;

namespace Test.SocketModels.Models
{
    public class ResponseModel : ISerializableModel
    {
        [JsonProperty("type")]
        [JsonPropertyName("type")]
        public int Type { get; set; } = 101;

        [JsonProperty("delay")]
        [JsonPropertyName("delay")]
        public int Delay { get; set; }

        [JsonProperty("value")]
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}