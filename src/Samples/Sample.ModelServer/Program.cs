using System.Threading.Tasks;
using Horse.Server;
using Horse.WebSocket.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.ModelServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            HorseServer server = new HorseServer();

            server.AddWebSockets(cfg => cfg.AddBus(services)
                                           .AddSingletonHandlers(typeof(Program)));

            server.UseWebSockets(services.BuildServiceProvider());
            server.Run(9999);
        }
    }
}