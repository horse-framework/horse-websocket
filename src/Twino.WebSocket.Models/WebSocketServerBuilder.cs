using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twino.Core;
using Twino.Ioc;
using Twino.Protocols.WebSocket;

namespace Twino.WebSocket.Models
{
    /// <summary>
    /// WebSocket Server builder
    /// </summary>
    public class WebSocketServerBuilder
    {
        private readonly ModelWsConnectionHandler _handler;

        internal WebSocketServerBuilder()
        {
            _handler = new ModelWsConnectionHandler();
        }

        internal ModelWsConnectionHandler Build()
        {
            return _handler;
        }

        #region Events

        /// <summary>
        /// Action to handle client connections and decide client type
        /// </summary>
        public WebSocketServerBuilder OnClientConnected(Func<IConnectionInfo, ConnectionData, Task<WsServerSocket>> func)
        {
            _handler.ConnectedFunc = func;
            return this;
        }

        /// <summary>
        /// Action to handle client ready status
        /// </summary>
        public WebSocketServerBuilder OnClientReady(Func<WsServerSocket, Task> action)
        {
            _handler.ReadyAction = action;
            return this;
        }

        /// <summary>
        /// Action to handle client disconnections
        /// </summary>
        public WebSocketServerBuilder OnClientDisconnected(Func<WsServerSocket, Task> action)
        {
            _handler.DisconnectedAction = action;
            return this;
        }

        /// <summary>
        /// Action to handle received messages
        /// </summary>
        public WebSocketServerBuilder OnMessageReceived(Func<WebSocketMessage, WsServerSocket, Task> action)
        {
            _handler.MessageReceivedAction = action;
            return this;
        }

        /// <summary>
        /// Action to handle errors
        /// </summary>
        public WebSocketServerBuilder OnError(Action<Exception> action)
        {
            _handler.ErrorAction = action;
            return this;
        }

        #endregion

        #region Register

        /// <summary>
        /// Uses custom model provider
        /// </summary>
        public WebSocketServerBuilder UseModelProvider(IWebSocketModelProvider provider)
        {
            _handler.Observer = new WebSocketMessageObserver(provider, _handler.Observer.ErrorAction);
            return this;
        }

        /// <summary>
        /// Registers IWebSocketMessageHandlers.
        /// All handlers must have parameterless constructor.
        /// If you need to inject services, use overloads.
        /// </summary>
        public WebSocketServerBuilder RegisterHandlers(params Type[] assemblyTypes)
        {
            _handler.Observer.RegisterWebSocketHandlers(null, assemblyTypes);
            return this;
        }

        /// <summary>
        /// Registers a handler as transient
        /// </summary>
        public WebSocketServerBuilder RegisterTransientHandler(ITwinoServiceCollection services, Type handlerType)
        {
            return RegisterHandler(services, ImplementationType.Transient, handlerType);
        }

        /// <summary>
        /// Registers a handler as scoped
        /// </summary>
        public WebSocketServerBuilder RegisterScopedHandler(ITwinoServiceCollection services, Type handlerType)
        {
            return RegisterHandler(services, ImplementationType.Scoped, handlerType);
        }

        /// <summary>
        /// Registers a handler as singleton
        /// </summary>
        public WebSocketServerBuilder RegisterSingletonHandler(ITwinoServiceCollection services, Type handlerType)
        {
            return RegisterHandler(services, ImplementationType.Singleton, handlerType);
        }


        /// <summary>
        /// Registers handlers as transient
        /// </summary>
        public WebSocketServerBuilder RegisterTransientHandlers(ITwinoServiceCollection services, params Type[] assemblyTypes)
        {
            return RegisterHandlers(services, ImplementationType.Transient, assemblyTypes);
        }

        /// <summary>
        /// Registers handlers as scoped
        /// </summary>
        public WebSocketServerBuilder RegisterScopedHandlers(ITwinoServiceCollection services, params Type[] assemblyTypes)
        {
            return RegisterHandlers(services, ImplementationType.Scoped, assemblyTypes);
        }

        /// <summary>
        /// Registers handlers as singleton
        /// </summary>
        public WebSocketServerBuilder RegisterSingletonHandlers(ITwinoServiceCollection services, params Type[] assemblyTypes)
        {
            return RegisterHandlers(services, ImplementationType.Singleton, assemblyTypes);
        }

        /// <summary>
        /// Registers handlers
        /// </summary>
        private WebSocketServerBuilder RegisterHandler(ITwinoServiceCollection services, ImplementationType implementation, Type handlerType)
        {
            IServiceContainer container = (IServiceContainer) services;

            if (!container.Contains(typeof(IWebSocketServerBus)))
                container.AddSingleton<IWebSocketServerBus>(_handler);
            
            _handler.Observer.RegisterWebSocketHandler(handlerType, t => container.Get(t));
            Extensions.AddHandlerIntoContainer(container, implementation, handlerType);
            return this;
        }

        /// <summary>
        /// Registers handlers
        /// </summary>
        private WebSocketServerBuilder RegisterHandlers(ITwinoServiceCollection services, ImplementationType implementation, params Type[] assemblyTypes)
        {
            IServiceContainer container = (IServiceContainer) services;
            
            if (!container.Contains(typeof(IWebSocketServerBus)))
                container.AddSingleton<IWebSocketServerBus>(_handler);
            
            List<Type> types = _handler.Observer.RegisterWebSocketHandlers(t => container.Get(t), assemblyTypes);
            foreach (Type type in types)
                Extensions.AddHandlerIntoContainer(container, implementation, type);

            return this;
        }

        /// <summary>
        /// Gets message bus of websocket server
        /// </summary>
        /// <returns></returns>
        public WebSocketServerBuilder AddBus(ITwinoServiceCollection services)
        {
            IServiceContainer container = (IServiceContainer) services;
            
            if (!container.Contains(typeof(IWebSocketServerBus)))
                container.AddSingleton<IWebSocketServerBus>(_handler);

            return this;
        }
        
        /// <summary>
        /// Gets message bus of websocket server
        /// </summary>
        /// <returns></returns>
        public IWebSocketServerBus GetBus()
        {
            return _handler;
        }

        #endregion
    }
}