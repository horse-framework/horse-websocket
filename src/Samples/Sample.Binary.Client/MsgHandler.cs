using Horse.WebSocket.Protocol;

namespace Sample.Binary.Client;

public class MsgHandler : IWebSocketMessageHandler<TestModel>
{
    public ValueTask Handle(TestModel model, WebSocketMessage message, IHorseWebSocket client)
    {
        Console.WriteLine("Received: " + model.Item3 + " -> " + message.OpCode);
        return ValueTask.CompletedTask;
    }

    public ValueTask OnError(Exception exception, TestModel model, WebSocketMessage message, IHorseWebSocket client)
    {
        return ValueTask.CompletedTask;
    }
}