using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Horse.Core;
using Horse.Core.Protocols;
using Horse.Server;
using Horse.WebSocket.Protocol;
using Horse.WebSocket.Protocol.Security;
using Horse.WebSocket.Server;
using Microsoft.Extensions.Hosting;
using HostOptions = Horse.Server.HostOptions;

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
            AesGcmMessageEncryptor gcm = new AesGcmMessageEncryptor();
            ChaCha20Poly1305Encryptor cc = new ChaCha20Poly1305Encryptor();
            gcm.SetKeys(Encoding.UTF8.GetBytes("12345123451234512345123451234512"), Encoding.UTF8.GetBytes("123451234512"), Encoding.UTF8.GetBytes("1234512345123451"));
            cc.SetKeys(Encoding.UTF8.GetBytes("12345123451234512345123451234512"), Encoding.UTF8.GetBytes("123451234512"), Encoding.UTF8.GetBytes("1234512345123451"));
            var msg = WebSocketMessage.FromString("Hello, world!");

            gcm.EncryptMessage(msg);
            gcm.DecryptMessage(msg);
            cc.EncryptMessage(msg);
            cc.DecryptMessage(msg);
            Console.ReadLine();

            while (true)
            {
                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 0; i < 1000000; i++)
                {
                    gcm.EncryptMessage(msg);
                    gcm.DecryptMessage(msg);
                }

                sw.Stop();
                Console.Write("Elapsed GCM " + sw.ElapsedMilliseconds);
                Console.ReadLine();

                sw = Stopwatch.StartNew();
                for (int i = 0; i < 1000000; i++)
                {
                    cc.EncryptMessage(msg);
                    cc.DecryptMessage(msg);
                }

                sw.Stop();
                Console.Write("CC GCM " + sw.ElapsedMilliseconds);
                Console.ReadLine();
            }

            return;

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

            AesMessageEncryptor encryptor = new AesMessageEncryptor();
            byte[] key = new byte[16];
            byte[] iv = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                key[i] = (byte) (i + 100);
                iv[i] = (byte) (i + 50);
            }

            encryptor.SetKeys(key, iv);

            IHost host = Host.CreateDefaultBuilder()
                .UseHorseWebSocketServer(server, 4083, handler)
                .Build();

            host.Run();

            while (true)
            {
                Console.ReadLine();
                Console.WriteLine(handler.Online + " Online");
            }
        }
    }
}