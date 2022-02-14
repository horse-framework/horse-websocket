using System;
using System.Threading.Tasks;
using Horse.Mvc;
using Horse.Mvc.Controllers;
using Horse.Mvc.Filters.Route;
using Horse.Server;
using Horse.WebSocket.Client;
using Horse.WebSocket.Server;

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
            HorseServer server = new HorseServer(ServerOptions.CreateDefault());
            server.UseWebSockets(async (socket, data) =>
                {
                    Console.WriteLine("connected");
                    socket.Disconnected += c => Console.WriteLine("disconnected");
                    await Task.CompletedTask;
                },
                async (socket, message) =>
                {
                    Console.Write(message);
                    await socket.SendAsync(message);
                });

            server.Options.PingInterval = 30;
            server.Start();
        }

        static void ConnectWithHorse()
        {
            HorseWebSocketConnection client = new HorseWebSocketConnection();
            client.MessageReceived += (c, m) => Console.WriteLine("# " + m);
            client.Connected += c => Console.WriteLine("Connected");
            client.Disconnected += c => Console.WriteLine("Disconnected");
            client.Connect("ws://127.0.0.1:4083");
            //client.Connect("wss://echo.websocket.org");
            client.PingInterval = TimeSpan.FromSeconds(5);

            while (true)
            {
                string s = Console.ReadLine();
                client.Send(s);
            }
        }

        static void Main(string[] args)
        {
            // StartServer();
            ConnectWithHorse();
        }
    }
}