using System;
using Horse.Client.Connectors;
using Horse.Client.WebSocket;
using Horse.Client.WebSocket.Connectors;
using Horse.Protocols.WebSocket;

namespace Horse.WebSocket.Models.Internal
{
    /// <summary>
    /// Wrapper class for event action.
    /// Used to prevent holding larger builder object in memory because of anonymous lambda function references
    /// </summary>
    internal class ExceptionEventMapper
    {
        private readonly WsStickyConnector _connector;
        private readonly Action<Exception> _action;

        /// <summary>
        /// Creates new connection event wrapper
        /// </summary>
        public ExceptionEventMapper(WsStickyConnector connector, Action<Exception> action)
        {
            _connector = connector;
            _action = action;
        }

        /// <summary>
        /// Event action mapper
        /// </summary>
        /// <returns></returns>
        public void Action(IConnector<HorseWebSocket, WebSocketMessage> c, Exception e)
        {
            _action(e);
        }
    }
}