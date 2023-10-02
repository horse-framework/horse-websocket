using Horse.WebSocket.Protocol.Providers;
using Horse.WebSocket.Server;
using Microsoft.Extensions.Hosting;

IHost host = Host.CreateDefaultBuilder(args)
    .UseHorseWebSocketServer((context, builder) =>
    {
        builder.UseModelProvider<BinaryModelProvider>();
        builder.AddTransientHandlers(typeof(Program));
        builder.UsePort(888);
    })
    .Build();

host.Run();