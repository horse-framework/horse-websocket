using Twino.WebSocket.Models;

namespace Sample.ModelServer.Models
{
    [ModelType("model-a")]
    public class ModelA
    {
        public string Value { get; set; }
    }
}