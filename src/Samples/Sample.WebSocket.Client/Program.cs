using System;
using System.Threading.Tasks;
using Horse.Mvc;
using Horse.Mvc.Controllers;
using Horse.Mvc.Filters.Route;
using Horse.Server;
using Horse.WebSocket.Client;
using Horse.WebSocket.Protocol;
using Horse.WebSocket.Protocol.Security;
using Horse.WebSocket.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Sample.WebSocket.Client
{
    [Route("")]
    public class TController : HorseController
    {
        [HttpGet("")]
        public IActionResult Get()
        {
            return String(".");
        }
    }

    class Program
    {
        static void StartServer()
        {
            IHost host = Host.CreateDefaultBuilder()
                .UseHorseWebSocketServer(80,
                    (socket, data) =>
                    {
                        Console.WriteLine("connected");
                        socket.Disconnected += c => Console.WriteLine("disconnected");
                        return Task.CompletedTask;
                    },
                    _ => Task.CompletedTask,
                    async (socket, message) =>
                    {
                        Console.Write(message);
                        await socket.SendAsync(message);
                    })
                .Build();

            HorseServer server = host.Services.GetService<HorseServer>();
            server.Options.PingInterval = 30;

            host.Run();
        }

        static void ConnectWithHorse()
        {
            AesMessageEncryptor encryptor = new AesMessageEncryptor();
            byte[] key = new byte[16];
            byte[] iv = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                key[i] = (byte) (i + 100);
                iv[i] = (byte) (i + 50);
            }

            encryptor.SetKeys(key, iv);

            HorseWebSocket client = new HorseWebSocket();
            //client.Encryptor = encryptor;
            //HorseWebSocketConnection client = new HorseWebSocketConnection();
            client.MessageReceived += (c, m) => Console.WriteLine("# " + m);
            client.Connected += c => Console.WriteLine("Connected");
            client.Disconnected += c => Console.WriteLine("Disconnected");
            client.ConnectAsync("ws://127.0.0.1:4083");
            //client.ConnectAsync("wss://echo.websocket.org");

            while (true)
            {
                string s = Console.ReadLine();
                client.Connection.Send(s);
            }
        }

        static void Main(string[] args)
        {
            // StartServer();
            ConnectWithHorse();
        }
    }
}