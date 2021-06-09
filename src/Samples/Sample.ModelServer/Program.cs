using System;
using System.Net;
using System.Threading.Tasks;
using Horse.Protocols.Http;
using Horse.Protocols.WebSocket;
using Horse.Server;
using Horse.WebSocket.Models;
using Horse.WebSocket.Models.Providers;
using Horse.WebSocket.Models.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Sample.ModelServer.Models;

namespace Sample.ModelServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            HorseServer server = new HorseServer();

            server.AddWebSockets(cfg => cfg.AddBus(services)
                                           //.UsePipeModelProvider(new NewtonsoftJsonModelSerializer())
                                           .UsePayloadModelProvider(new SystemJsonModelSerializer())
                                           .AddSingletonHandlers(typeof(Program))
                                           /*
                                           .OnClientConnected((info, data) =>
                                           {
                                               WsServerSocket socket = new YourDerivedCustomSocket(info, data);
                                               Task.FromResult(socket);
                                           })
                                           */
                                           .OnClientReady(client =>
                                           {
                                               Console.WriteLine("Client connected");
                                               return Task.CompletedTask;
                                           })
                                           .OnClientDisconnected(client =>
                                           {
                                               Console.WriteLine("Client disconnected");
                                               return Task.CompletedTask;
                                           })
                                           .OnError(exception => Console.WriteLine("Error: " + exception)));

            server.UseWebSockets(services.BuildServiceProvider());

            server.UseHttp((request, response) =>
            {
                if (request.Path.Equals("/status", StringComparison.InvariantCultureIgnoreCase))
                {
                    response.SetToText();
                    response.StatusCode = HttpStatusCode.OK;
                    response.Write("OK");
                }
                else
                    response.StatusCode = HttpStatusCode.NotFound;

                return Task.CompletedTask;
            });

            server.Run(26111);
        }
    }
}