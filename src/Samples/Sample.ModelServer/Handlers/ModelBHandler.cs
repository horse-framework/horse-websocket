using System;
using System.Threading.Tasks;
using Horse.WebSocket.Protocol;
using Sample.ModelServer.Models;

namespace Sample.ModelServer.Handlers
{
    public class ModelBHandler : IWebSocketMessageHandler<ModelB, WsServerSocket>
    {
        public Task Handle(ModelB model, WebSocketMessage message, WsServerSocket client)
        {
            Console.WriteLine("model b received: " + model.Foo);
            return Task.CompletedTask;
        }

        public Task OnError(Exception exception, ModelB model, WebSocketMessage message, WsServerSocket client)
        {
            throw new NotImplementedException();
        }
    }
}