using System;
using System.Threading.Tasks;
using Sample.ModelServer.Models;
using Twino.Protocols.WebSocket;
using Twino.WebSocket.Models;

namespace Sample.ModelServer.Handlers
{
    public class ModelBHandler : IWebSocketMessageHandler<ModelB>
    {
        public Task<object> Handle(ModelB model, WebSocketMessage message, ITwinoWebSocket client)
        {
            Console.WriteLine("model b received: " + model.Foo);
            return null;
        }

        public async Task<object> OnError(Exception exception, ModelB model, WebSocketMessage message, ITwinoWebSocket client)
        {
            throw new NotImplementedException();
        }
    }
}