using System.Threading.Tasks;
using Test.SocketModels.Helpers;
using Test.SocketModels.Models;
using Horse.Client.WebSocket;
using Horse.SerializableModel;
using Xunit;

namespace Test.SocketModels
{
    public class PackageReaderTest
    {
        [Fact]
        public void Single()
        {
            DefaultModel received = null;

            PackageReader reader = new PackageReader();
            reader.On<DefaultModel>((sender, model) => received = model);

            TestServer server = new TestServer(44351);
            server.Run(reader);

            System.Threading.Thread.Sleep(250);

            HorseWebSocket client = new HorseWebSocket();
            client.Connect("127.0.0.1", 44351, false);

            client.Send(new DefaultModel { Name = "Mehmet", Number = 500 });

            //wait for async package reading
            System.Threading.Thread.Sleep(2000);

            Assert.NotNull(received);
            Assert.Equal(500, received.Number);
        }

        [Fact]
        public async Task Multiple()
        {
            DefaultModel received1 = null;
            CriticalModel received2 = null;

            PackageReader reader1 = new PackageReader();
            PackageReader reader2 = new PackageReader();
            reader1.On<DefaultModel>((sender, model) => received1 = model);
            reader1.On<CriticalModel>((sender, model) => received2 = model);

            TestServer server = new TestServer(44352);
            server.Run(reader1, reader2);

            HorseWebSocket client = new HorseWebSocket();
            client.Connect("127.0.0.1", 44352, false);

            client.Send(new DefaultModel { Name = "Default", Number = 501 });
            client.Send(new CriticalModel { Name = "Critical", Number = 502 });

            //wait for async package reading
            await Task.Delay(2000);

            Assert.NotNull(received1);
            Assert.Equal(501, received1.Number);

            Assert.NotNull(received2);
            Assert.Equal(502, received2.Number);
        }
    }
}