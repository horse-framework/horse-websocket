using System.Linq;
using System.Threading.Tasks;
using Horse.Protocols.WebSocket;
using Horse.SerializableModel;
using Horse.Server;

namespace Test.SocketModels.Helpers
{
    public class TestServer
    {
        private readonly int _port;

        public HorseServer Server { get; private set; }

        public TestServer(int port)
        {
            _port = port;
        }

        public void Run(params PackageReader[] readers)
        {
            ServerOptions options = ServerOptions.CreateDefault();
            options.Hosts.FirstOrDefault().Port = _port;

            Server = new HorseServer(ServerOptions.CreateDefault());
            Server.UseWebSockets(async (socket, message) =>
            {
                string msg = message.ToString();

                foreach (PackageReader reader in readers)
                    reader.Read(socket, msg);

                await Task.CompletedTask;
            });

            Server.Start(_port);
        }
    }
}