using System;
using Twino.Client.WebSocket;

namespace Twino.WebSocket.Models.Internal
{
    /// <summary>
    /// Mapper class for event action.
    /// Used to prevent holding larger builder object in memory because of anonymous lambda function references
    /// </summary>
    internal class ConnectionEventMapper
    {
        private readonly WebSocketModelConnector _connector;
        private readonly Action<WebSocketModelConnector> _action;

        /// <summary>
        /// Creates new connection event wrapper
        /// </summary>
        public ConnectionEventMapper(WebSocketModelConnector connector, Action<WebSocketModelConnector> action)
        {
            _connector = connector;
            _action = action;
        }

        /// <summary>
        /// Event action mapper
        /// </summary>
        /// <returns></returns>
        public void Action(TwinoWebSocket client)
        {
            _action(_connector);
        }
    }
}