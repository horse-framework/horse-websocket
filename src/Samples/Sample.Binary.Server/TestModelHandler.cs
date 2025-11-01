using Horse.WebSocket.Protocol;

namespace Sample.Binary.Server;

public class TestModelHandler : IWebSocketMessageHandler<TestModel>
{
    public Task Handle(TestModel model, WebSocketMessage message, IHorseWebSocket client)
    {
        Console.WriteLine("Received: " + model.Item3 + " > " + message.OpCode);
        return client.SendAsync(message).AsTask();
    }
}