using System;
using System.Threading.Tasks;
using Twino.Core;
using Twino.Ioc;
using Twino.Server;
using Twino.WebSocket.Models;

namespace Sample.ModelServer
{
    class Program
    {
        static Task Main(string[] args)
        {
            IServiceContainer services = new ServiceContainer();
            TwinoServer server = new TwinoServer();
            server.UseWebSockets(cfg => cfg.AddSingletonHandlers(services, typeof(Program)));
            server.Start(9999);
            return server.BlockWhileRunningAsync();
        }
    }
}