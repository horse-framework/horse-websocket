using System.Threading.Tasks;

namespace Horse.Protocols.WebSocket
{
    /// <summary>
    /// WebSocket client implementation
    /// </summary>
    public interface IHorseWebSocket
    {
        /// <summary>
        /// Sends a websocket message
        /// </summary>
        Task<bool> SendAsync(WebSocketMessage message);

        /// <summary>
        /// Closes client connection
        /// </summary>
        void Disconnect();
    }
}