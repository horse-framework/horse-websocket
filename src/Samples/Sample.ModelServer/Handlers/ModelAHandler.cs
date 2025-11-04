using System;
using System.Threading.Tasks;
using Horse.WebSocket.Protocol;
using Sample.ModelServer.Models;

namespace Sample.ModelServer.Handlers
{
    public class ModelAHandler : IWebSocketMessageHandler<ModelA, WsServerSocket>
    {
        public ValueTask Handle(ModelA model, WebSocketMessage message, WsServerSocket client)
        {
            Console.WriteLine("Model A received: " + model.Value);
            return ValueTask.CompletedTask;
        }

        public ValueTask OnError(Exception exception, ModelA model, WebSocketMessage message, WsServerSocket client)
        {
            throw new NotImplementedException();
        }
    }
}