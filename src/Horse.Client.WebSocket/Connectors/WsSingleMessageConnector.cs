using System;
using Horse.Client.Connectors;
using Horse.Protocols.WebSocket;

namespace Horse.Client.WebSocket.Connectors
{
    /// <summary>
    /// Single message connector for websocket.
    /// </summary>
    public class WsSingleMessageConnector : SingleMessageConnector<HorseWebSocket, WebSocketMessage>
    {
        /// <summary>
        /// Creates new single message connector for websocket connections
        /// </summary>
        public WsSingleMessageConnector(Func<HorseWebSocket> createInstance = null)
            : base(createInstance)
        {
        }
    }
}