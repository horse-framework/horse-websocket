using System;
using System.Net;
using System.Threading.Tasks;
using Horse.Protocols.Http;
using Horse.Server;
using Horse.WebSocket.Models;
using Horse.WebSocket.Models.Serialization;
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
                                           .UsePipeModelProvider(new NewtonsoftJsonModelSerializer())
                                           .AddTransientHandlers(typeof(Program))
                                           .OnError(exception => { Console.WriteLine("Error: " + exception); }));

            server.UseWebSockets(services.BuildServiceProvider());

            server.UseHttp((request, response) =>
            {
                response.StatusCode = HttpStatusCode.Accepted;
                return Task.CompletedTask;
            });

            server.Run(9999);
        }
    }
}