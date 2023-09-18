using Horse.WebSocket.Protocol;

namespace Sample.Binary.Client;

public class MsgHandler : IWebSocketMessageHandler<TestModel>
{
    public Task Handle(TestModel model, WebSocketMessage message, IHorseWebSocket client)
    {
        return Task.CompletedTask;
    }

    public Task OnError(Exception exception, TestModel model, WebSocketMessage message, IHorseWebSocket client)
    {
        return Task.CompletedTask;
    }
}