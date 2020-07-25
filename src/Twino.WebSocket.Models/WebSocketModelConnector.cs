using System;
using Twino.Client.WebSocket;
using Twino.Client.WebSocket.Connectors;

namespace Twino.WebSocket.Models
{
    public class WebSocketModelConnector : WsStickyConnector
    {
        public WebSocketMessageConsumer Consumer { get; private set; }

        internal IWebSocketModelResolver ModelResolver { get; set; }
        internal IWebSocketModelReader ModelReader { get; set; }
        internal IWebSocketModelWriter ModelWriter { get; set; }

        public WebSocketModelConnector(TimeSpan reconnectInterval, Func<TwinoWebSocket> createInstance = null)
            : base(reconnectInterval, createInstance)
        {
        }
    }
}