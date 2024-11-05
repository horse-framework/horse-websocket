using System;
using System.Threading.Tasks;
using Horse.WebSocket.Client;
using Horse.WebSocket.Protocol.Security;

namespace Sample.WebSocket.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var enc = new AesMessageEncryptor();
            enc.EncryptorId = 3;
            byte[] key = new byte[32];
            byte[] iv = new byte[16];
            for (int i = 0; i < 32; i++)
            {
                key[i] = (byte) (i + 45);
                if (i < iv.Length)
                    iv[i] = (byte) (i + 22);
            }

            enc.SetKeys(key, iv);

            HorseWebSocket client = new HorseWebSocket();
            client.EncryptorContainer.SetDefaultEncryptor(enc);

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