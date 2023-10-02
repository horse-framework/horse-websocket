# Horse WebSocket

[![NuGet](https://img.shields.io/nuget/v/Horse.WebSocket.Client?label=client%20nuget)](https://www.nuget.org/packages/Horse.WebSocket.Client)
[![NuGet](https://img.shields.io/nuget/v/Horse.WebSocket.Server?label=server%20nuget)](https://www.nuget.org/packages/Horse.WebSocket.Server)

Horse WebSocket includes websocket server and websocket client. Websocket servers runs on Horse Server.

#### Basic WebSocket Server Example

```C#
IHost host = Host.CreateDefaultBuilder(args)
    .UseHorseWebSocketServer((context, builder) =>
    {
        builder.UseModelProvider<BinaryModelProvider>();
        builder.AddTransientHandlers(typeof(Program));
        builder.UsePort(888);
    })
    .Build();

host.Run();
```

#### Basic WebSocket Client Example

```C#
HorseWebSocket client = new HorseWebSocket();
client.MessageReceived += (c, m) => Console.WriteLine("# " + m);
client.Connected += c => Console.WriteLine("Connected");
client.Disconnected += c => Console.WriteLine("Disconnected");
client.Connect("wss://echo.websocket.org");

while (true)
{
    string s = Console.ReadLine();
    client.Send(s);
}
```
     
**Handler Implementation**

```C#
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
```
    
**Model**

```C#
[ModelType("model-a")]
public class ModelA
{
    [JsonProperty("v")]
    public string Value { get; set; }
}
```

