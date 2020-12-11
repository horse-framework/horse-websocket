using System;
using Horse.Client.Connectors;
using Horse.Protocols.WebSocket;

namespace Horse.Client.WebSocket.Connectors
{
    /// <summary>
    /// Absolute connector for websocket.
    /// </summary>
    public class WsAbsoluteConnector : AbsoluteConnector<HorseWebSocket, WebSocketMessage>
    {
        /// <summary>
        /// Creates new absolute connector for websocket connections
        /// </summary>
        public WsAbsoluteConnector(TimeSpan reconnectInterval, Func<HorseWebSocket> createInstance = null)
            : base(reconnectInterval, createInstance)
        {
        }
    }
}