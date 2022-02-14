using System;
using System.Threading.Tasks;
using Horse.WebSocket.Protocol;
using Sample.ModelServer.Models;

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