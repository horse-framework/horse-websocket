using Horse.WebSocket.Protocol;

namespace Sample.Binary.Server;

public class TestModelHandler : IWebSocketMessageHandler<TestModel>
{
    public async ValueTask Handle(TestModel model, WebSocketMessage message, IHorseWebSocket client)
    {
        Console.WriteLine("Received: " + model.Item3 + " > " + message.OpCode);
        await client.SendAsync(message);
    }
}