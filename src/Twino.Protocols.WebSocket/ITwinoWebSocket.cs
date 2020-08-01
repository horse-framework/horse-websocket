using System.Threading.Tasks;

namespace Twino.Protocols.WebSocket
{
    /// <summary>
    /// WebSocket client implementation
    /// </summary>
    public interface ITwinoWebSocket
    {
        /// <summary>
        /// Sends a websocket message
        /// </summary>
        Task<bool> SendAsync(WebSocketMessage message);
    }
}