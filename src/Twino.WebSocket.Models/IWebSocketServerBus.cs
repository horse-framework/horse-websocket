using System.Threading.Tasks;
using Twino.Protocols.WebSocket;

namespace Twino.WebSocket.Models
{
    /// <summary>
    /// Message Bus for websocket servers
    /// </summary>
    public interface IWebSocketServerBus
    {
        /// <summary>
        /// Sends a message over websocket
        /// </summary>
        Task<bool> SendAsync<TModel>(WsServerSocket target, TModel model);

        /// <summary>
        /// Removes client from server
        /// </summary>
        void Disconnect(WsServerSocket client);
    }
}