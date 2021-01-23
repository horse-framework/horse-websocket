using System;
using Horse.Client.Connectors;
using Horse.Protocols.WebSocket;

namespace Horse.Client.WebSocket.Connectors
{
    /// <summary>
    /// Necessity connector for websocket.
    /// </summary>
    public class WsNecessityConnector : NecessityConnector<HorseWebSocket, WebSocketMessage>
    {
        /// <summary>
        /// Creates new necessity connector for websocket connections
        /// </summary>
        public WsNecessityConnector(Func<HorseWebSocket> createInstance = null)
            : base(createInstance)
        {
        }
    }
}