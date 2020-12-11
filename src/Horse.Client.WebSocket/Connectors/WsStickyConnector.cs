using System;
using Horse.Client.Connectors;
using Horse.Protocols.WebSocket;

namespace Horse.Client.WebSocket.Connectors
{
    /// <summary>
    /// Sticky connector for websocket.
    /// </summary>
    public class WsStickyConnector : StickyConnector<HorseWebSocket, WebSocketMessage>
    {
        /// <summary>
        /// Creates new sticky connector for websocket connections
        /// </summary>
        public WsStickyConnector(TimeSpan reconnectInterval, Func<HorseWebSocket> createInstance = null)
            : base(reconnectInterval, createInstance)
        {
        }
    }
}