using Horse.WebSocket.Protocol;
using Horse.WebSocket.Server;

namespace Sample.Binary.Server;

public class MsgHandler : IWebSocketMessageHandler<TestModel>
{
    private readonly IWebSocketServerBus _bus;

    public MsgHandler(IWebSocketServerBus bus)
    {
        _bus = bus;
    }

    public async Task Handle(TestModel model, WebSocketMessage message, IHorseWebSocket client)
    {
        await _bus.SendAsync(client, model);
    }

    public Task OnError(Exception exception, TestModel model, WebSocketMessage message, IHorseWebSocket client)
    {
        return Task.CompletedTask;
    }
}