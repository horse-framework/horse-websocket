using System;
using System.Threading.Tasks;
using Horse.Protocols.WebSocket;
using Sample.ModelServer.Models;
using Horse.WebSocket.Models;

namespace Sample.ModelServer.Handlers
{
    public class ModelAHandler : IWebSocketMessageHandler<ModelA>
    {
        public Task Handle(ModelA model, WebSocketMessage message, IHorseWebSocket client)
        {
            Console.WriteLine("Model A received: " + model.Value);
            return Task.CompletedTask;
        }

        public Task OnError(Exception exception, ModelA model, WebSocketMessage message, IHorseWebSocket client)
        {
            throw new NotImplementedException();
        }
    }
}