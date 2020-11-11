using System;
using System.Threading.Tasks;
using Twino.Client.WebSocket;
using Twino.Client.WebSocket.Connectors;
using Twino.Core;
using Twino.Protocols.WebSocket;

namespace Twino.WebSocket.Models
{
    /// <summary>
    /// Sticky Websocket connector with unique identifier to register multiple client buses into same ioc container.
    /// If you are using only one bus in a container, use non-generic instead.
    /// </summary>
    public class WebSocketModelConnector<TIdentifier> : WebSocketModelConnector, IWebSocketBus<TIdentifier>
    {
        /// <summary>
        /// Creates new websocket model connector
        /// </summary>
        public WebSocketModelConnector(TimeSpan reconnectInterval, Func<TwinoWebSocket> createInstance = null)
            : base(reconnectInterval, createInstance)
        {
        }
    }

    /// <summary>
    /// Sticky Websocket connector with model provider
    /// </summary>
    public class WebSocketModelConnector : WsStickyConnector, IWebSocketBus
    {
        /// <summary>
        /// Message observer
        /// </summary>
        public WebSocketMessageObserver Observer { get; internal set; }

        internal IWebSocketModelProvider ModelProvider { get; set; }

        internal IServiceProvider ServiceProvider { get; set; }
        
        internal TwinoWebSocketBuilder Builder { get; set; }

        /// <summary>
        /// Creates new websocket model connector
        /// </summary>
        public WebSocketModelConnector(TimeSpan reconnectInterval, Func<TwinoWebSocket> createInstance = null)
            : base(reconnectInterval, createInstance)
        {
        }

        /// <summary>
        /// Triggered when a message is received
        /// </summary>
        protected override void ClientMessageReceived(ClientSocketBase<WebSocketMessage> client, WebSocketMessage payload)
        {
            base.ClientMessageReceived(client, payload);

            if (Observer != null)
                Observer.Read(payload, (ITwinoWebSocket) client);
        }

        /// <summary>
        /// Sends a message over websocket
        /// </summary>
        public Task<bool> SendAsync<TModel>(TModel model)
        {
            WebSocketMessage message = ModelProvider.Write(model);
            return GetClient().SendAsync(message);
        }
    }
}