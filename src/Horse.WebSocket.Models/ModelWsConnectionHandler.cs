using System;
using System.Threading.Tasks;
using Horse.Core;
using Horse.Core.Protocols;
using Horse.Protocols.WebSocket;
using Horse.WebSocket.Models.Internal;
using Horse.WebSocket.Models.Providers;

namespace Horse.WebSocket.Models
{
    /// <summary>
    /// Model based websocket connection handler
    /// </summary>
    internal sealed class ModelWsConnectionHandler : IWebSocketServerBus, IProtocolConnectionHandler<WsServerSocket, WebSocketMessage>
    {
        #region Properties

        /// <summary>
        /// Message observer for websocket connection handler
        /// </summary>
        public WebSocketMessageObserver Observer { get; internal set; }

        internal Func<IConnectionInfo, ConnectionData, Task<WsServerSocket>> ConnectedFunc { get; set; }
        internal Func<WsServerSocket, Task> ReadyAction { get; set; }
        internal Func<WebSocketMessage, WsServerSocket, Task> MessageReceivedAction { get; set; }
        internal Func<WsServerSocket, Task> DisconnectedAction { get; set; }

        private Action<Exception> _errorAction;
        
        internal Action<Exception> ErrorAction
        {
            get => _errorAction;
            set
            {
                _errorAction = value;
                if (Observer != null)
                    Observer.ErrorAction = value;
            }
        }
        
        internal IServiceProvider ServiceProvider { get; set; }

        #endregion

        internal ModelWsConnectionHandler()
        {
            Observer = new WebSocketMessageObserver(new PipeModelProvider(), ErrorAction);
        }

        #region Events

        /// <summary>
        /// Connected event
        /// </summary>
        public Task<WsServerSocket> Connected(IHorseServer server, IConnectionInfo connection, ConnectionData data)
        {
            if (ConnectedFunc != null)
                return ConnectedFunc(connection, data);

            return Task.FromResult(new WsServerSocket(server, connection));
        }

        /// <summary>
        /// Client ready event
        /// </summary>
        public Task Ready(IHorseServer server, WsServerSocket client)
        {
            if (ReadyAction != null)
                return ReadyAction(client);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Message received event
        /// </summary>
        public async Task Received(IHorseServer server, IConnectionInfo info, WsServerSocket client, WebSocketMessage message)
        {
            await Observer.Read(message, client);

            if (MessageReceivedAction != null)
                await MessageReceivedAction(message, client);
        }

        /// <summary>
        /// Client disconnected event
        /// </summary>
        public Task Disconnected(IHorseServer server, WsServerSocket client)
        {
            if (DisconnectedAction != null)
                return DisconnectedAction(client);

            return Task.CompletedTask;
        }

        #endregion

        #region Actions

        /// <summary>
        /// Sends a model to a receiver client
        /// </summary>
        public Task<bool> SendAsync<TModel>(WsServerSocket target, TModel model)
        {
            WebSocketMessage message = Observer.Provider.Write(model);
            return target.SendAsync(message);
        }

        /// <summary>
        /// Removes client from server
        /// </summary>
        public void Disconnect(WsServerSocket client)
        {
            client.Disconnect();
        }

        #endregion
    }
}