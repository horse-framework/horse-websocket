using System;
using System.Threading.Tasks;
using Horse.WebSocket.Client;

namespace Sample.WebSocket.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            HorseWebSocket client = new HorseWebSocket();
            client.MessageReceived += (c, m) => Console.WriteLine("# " + m);
            client.Connected += c => Console.WriteLine("Connected");
            client.Disconnected += c => Console.WriteLine("Disconnected");
            await client.ConnectAsync("ws://127.0.0.1:888");

            while (true)
            {
                string s = Console.ReadLine();
                client.Connection.Send(s);
            }
        }
    }
}