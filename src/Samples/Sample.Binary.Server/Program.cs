using Horse.WebSocket.Server;
using Microsoft.Extensions.Hosting;

IHost host = Host.CreateDefaultBuilder(args)
    .UseHorseWebSocketServer((context, builder) =>
    {
        builder.UsePipeModelProvider();
        builder.UseBinaryModelProvider();
        builder.AddTransientHandlers(typeof(Program));
        builder.UsePort(888);
    })
    .Build();

host.Run();