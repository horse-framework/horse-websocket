using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Horse.Core;
using Horse.Core.Protocols;
using Horse.Server;
using Horse.WebSocket.Protocol;
using Horse.WebSocket.Server;

namespace Sample.WebSocket.Server
{
    /// <summary>
    /// WebSocket Client Factory
    /// </summary>
    public class ServerWsHandler : IProtocolConnectionHandler<WsServerSocket, WebSocketMessage>
    {
        private int _online;

        public int Online => _online;

        public async Task<WsServerSocket> Connected(IHorseServer server, IConnectionInfo connection, ConnectionData data)
        {
            WsServerSocket socket = new WsServerSocket(server, connection);
            Interlocked.Increment(ref _online);
            return await Task.FromResult(socket);
        }

        public async Task Ready(IHorseServer server, WsServerSocket client)
        {
            await Task.CompletedTask;
        }

        public async Task Received(IHorseServer server, IConnectionInfo info, WsServerSocket client, WebSocketMessage message)
        {
            Console.WriteLine(message);
            await client.SendAsync("Response: " + message);
            await Task.CompletedTask;
        }

        public async Task Disconnected(IHorseServer server, WsServerSocket client)
        {
            Interlocked.Decrement(ref _online);
            await Task.CompletedTask;
        }
    }

    class Program
    {
        public static WsServerSocket ServerClient { get; set; }

        static void Main(string[] args)
        {
            ServerWsHandler handler = new ServerWsHandler();
            HorseServer server = new HorseServer(new ServerOptions
            {
                PingInterval = 15,
                Hosts = new List<HostOptions>
                {
                    new HostOptions
                    {
                        Port = 4083
                    }
                }
            });
            server.UseWebSockets(handler);
            server.Start();

            while (true)
            {
                Console.ReadLine();
                Console.WriteLine(handler.Online + " Online");
            }
        }
    }
}