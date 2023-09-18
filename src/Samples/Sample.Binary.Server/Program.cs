using Horse.Server;
using Horse.WebSocket.Protocol.Providers;
using Horse.WebSocket.Server;
using Microsoft.Extensions.DependencyInjection;

IServiceCollection services = new ServiceCollection();

HorseServer server = new HorseServer();

server.AddWebSockets(cfg =>
{
    cfg.UseMSDI(services);
    cfg.UseModelProvider<BinaryModelProvider>();
    cfg.AddTransientHandlers(typeof(Program));
});

var provider = services.BuildServiceProvider();
server.UseWebSockets(provider);

server.Run(888);