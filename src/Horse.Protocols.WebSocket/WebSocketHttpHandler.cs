using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Horse.Core;
using Horse.Core.Protocols;
using Horse.Protocols.Http;

[assembly: InternalsVisibleTo("Horse.WebSocket.Models")]

namespace Horse.Protocols.WebSocket
{
    /// <summary>
    /// Default websocekt connection handler
    /// </summary>
    internal class WebSocketHttpHandler : IProtocolConnectionHandler<SocketBase, HttpMessage>
    {
        /// <summary>
        /// Triggered when a websocket client is connected. 
        /// </summary>
        public async Task<SocketBase> Connected(IHorseServer server, IConnectionInfo connection, ConnectionData data)
        {
            return await Task.FromResult((SocketBase) null);
        }

        /// <summary>
        /// Triggered when a client sends a message to the server 
        /// </summary>
        public async Task Received(IHorseServer server, IConnectionInfo info, SocketBase client, HttpMessage message)
        {
            message.Response.StatusCode = HttpStatusCode.NotFound;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Triggered when handshake is completed and the connection is ready to communicate 
        /// </summary>
        public async Task Ready(IHorseServer server, SocketBase client)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Triggered when a websocket client is disconnected. 
        /// </summary>
        public async Task Disconnected(IHorseServer server, SocketBase client)
        {
            await Task.CompletedTask;
        }
    }
}