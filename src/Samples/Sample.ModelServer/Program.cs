using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Twino.Server;
using Twino.WebSocket.Models;

namespace Sample.ModelServer
{
    class Program
    {
        static Task Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            TwinoServer server = new TwinoServer();

            server.AddWebSockets(cfg => cfg.AddBus(services)
                                           .AddSingletonHandlers(typeof(Program)));

            server.UseWebSockets(services.BuildServiceProvider());
            
            server.Start(9999);
            
            return server.BlockWhileRunningAsync();
        }
    }
}