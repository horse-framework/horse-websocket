using System.Threading.Tasks;
using Horse.Protocols.WebSocket;
using Horse.Server;

namespace Test.Connector.Helpers
{
    public class TestServer
    {
        public void Start(int port)
        {
            HorseServer server = new HorseServer(ServerOptions.CreateDefault());

            server.UseWebSockets(async delegate { await Task.CompletedTask; });
            server.Start(port);
        }
    }
}