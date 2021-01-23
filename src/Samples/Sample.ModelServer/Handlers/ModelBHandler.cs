using System;
using System.Threading.Tasks;
using Horse.Protocols.WebSocket;
using Sample.ModelServer.Models;
using Horse.WebSocket.Models;

namespace Sample.ModelServer.Handlers
{
    public class ModelBHandler : IWebSocketMessageHandler<ModelB>
    {
        public Task Handle(ModelB model, WebSocketMessage message, IHorseWebSocket client)
        {
            Console.WriteLine("model b received: " + model.Foo);
            return Task.CompletedTask;
        }

        public Task OnError(Exception exception, ModelB model, WebSocketMessage message, IHorseWebSocket client)
        {
            throw new NotImplementedException();
        }
    }
}