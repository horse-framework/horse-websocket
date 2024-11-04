using System;
using System.Threading.Tasks;
using Horse.WebSocket.Protocol.Security;
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
                    builder.UseEncryption<AesMessageEncryptor>(cfg=>
                    {
                        cfg.Key = 3;
                        byte[] key = new byte[32];
                        byte[] iv = new byte[16];
                        for (int i = 0; i < 32; i++)
                        {
                            key[i] = (byte) (i + 45);
                            if (i < iv.Length)
                                iv[i] = (byte) (i + 22);
                        }

                        cfg.SetKeys(key, iv);
                    });
                    
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