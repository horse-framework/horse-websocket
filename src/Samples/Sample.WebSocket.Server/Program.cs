using System;
using System.Threading.Tasks;
using Horse.WebSocket.Server;
using Microsoft.Extensions.Hosting;

namespace Sample.WebSocket.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .UseHorseWebSocketServer((context, builder) =>
                {
                    builder.UsePort(888);
                    builder.OnClientReady((services, client) =>
                    {
                        Console.WriteLine("Client Connected");
                        return Task.CompletedTask;
                    });

                    builder.OnMessageReceived(async (services, message, socket) =>
                    {
                        Console.WriteLine("Received: " + message);
                        await socket.SendAsync(message);
                    });
                })
                .Build()
                .Run();
        }
    }
}