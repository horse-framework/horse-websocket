using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Horse.Core;
using Microsoft.Extensions.DependencyInjection;
using Horse.Protocols.WebSocket;
using Horse.WebSocket.Models.Providers;
using Horse.WebSocket.Models.Serialization;

namespace Horse.WebSocket.Models
{
    /// <summary>
    /// WebSocket Server builder
    /// </summary>
    public class WebSocketServerBuilder
    {
        private readonly ModelWsConnectionHandler _handler;

        private IServiceCollection _services;

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
        /// Uses pipe model provider
        /// </summary>
        public WebSocketServerBuilder UsePipeModelProvider(IJsonModelSerializer serializer = null)
        {
            if (serializer == null)
                serializer = new NewtonsoftJsonModelSerializer();

            if (_handler.Observer.HandlersRegistered)
                throw new InvalidOperationException("You must use Use...Provider methods before Add..Handler(s) methods. Change method call order.");

            _handler.Observer.Provider = new PipeModelProvider(serializer);

            if (_services != null)
                _services.AddSingleton(_handler.Observer.Provider);
            
            return this;
        }

        /// <summary>
        /// Uses payload model provider
        /// </summary>
        public WebSocketServerBuilder UsePayloadModelProvider(IJsonModelSerializer serializer = null)
        {
            if (serializer == null)
                serializer = new NewtonsoftJsonModelSerializer();

            if (_handler.Observer.HandlersRegistered)
                throw new InvalidOperationException("You must use Use...Provider methods before Add..Handler(s) methods. Change method call order.");

            _handler.Observer.Provider = new PayloadModelProvider(serializer);
            
            if (_services != null)
                _services.AddSingleton(_handler.Observer.Provider);
            
            return this;
        }

        /// <summary>
        /// Uses custom model provider
        /// </summary>
        public WebSocketServerBuilder UseModelProvider(IWebSocketModelProvider provider)
        {
            if (_handler.Observer.HandlersRegistered)
                throw new InvalidOperationException("You must use Use...Provider methods before Add..Handler(s) methods. Change method call order.");

            _handler.Observer.Provider = provider;
            
            if (_services != null)
                _services.AddSingleton(_handler.Observer.Provider);
            
            return this;
        }

        /// <summary>
        /// Uses custom model provider
        /// </summary>
        public WebSocketServerBuilder UseModelProvider<TWebSocketModelProvider>()
            where TWebSocketModelProvider : IWebSocketModelProvider, new()
        {
            if (_handler.Observer.HandlersRegistered)
                throw new InvalidOperationException("You must use Use...Provider methods before Add..Handler(s) methods. Change method call order.");
            
            _handler.Observer.Provider = new TWebSocketModelProvider();
            
            if (_services != null)
                _services.AddSingleton(_handler.Observer.Provider);
            
            return this;
        }

        /// <summary>
        /// Registers IWebSocketMessageHandlers.
        /// All handlers must have parameterless constructor.
        /// If you need to inject services, use overloads.
        /// </summary>
        public WebSocketServerBuilder AddHandlers(params Type[] assemblyTypes)
        {
            _handler.Observer.RegisterWebSocketHandlers(null, assemblyTypes);
            return this;
        }

        /// <summary>
        /// Registers a handler as transient
        /// </summary>
        public WebSocketServerBuilder AddTransientHandler(Type handlerType)
        {
            return AddHandler(ServiceLifetime.Transient, handlerType);
        }

        /// <summary>
        /// Registers a handler as scoped
        /// </summary>
        public WebSocketServerBuilder AddScopedHandler(Type handlerType)
        {
            return AddHandler(ServiceLifetime.Scoped, handlerType);
        }

        /// <summary>
        /// Registers a handler as singleton
        /// </summary>
        public WebSocketServerBuilder AddSingletonHandler(Type handlerType)
        {
            return AddHandler(ServiceLifetime.Singleton, handlerType);
        }


        /// <summary>
        /// Registers handlers as transient
        /// </summary>
        public WebSocketServerBuilder AddTransientHandlers(params Type[] assemblyTypes)
        {
            return AddHandlers(ServiceLifetime.Transient, assemblyTypes);
        }

        /// <summary>
        /// Registers handlers as scoped
        /// </summary>
        public WebSocketServerBuilder AddScopedHandlers(params Type[] assemblyTypes)
        {
            return AddHandlers(ServiceLifetime.Scoped, assemblyTypes);
        }

        /// <summary>
        /// Registers handlers as singleton
        /// </summary>
        public WebSocketServerBuilder AddSingletonHandlers(params Type[] assemblyTypes)
        {
            return AddHandlers(ServiceLifetime.Singleton, assemblyTypes);
        }

        /// <summary>
        /// Registers handlers
        /// </summary>
        private WebSocketServerBuilder AddHandler(ServiceLifetime lifetime, Type handlerType)
        {
            if (_services == null)
                throw new ArgumentNullException("ServiceCollection is not attached yet. Use AddBus method before adding handlers.");

            _handler.Observer.RegisterWebSocketHandler(handlerType, () => _handler.ServiceProvider);
            RegisterHandler(lifetime, handlerType);

            return this;
        }

        /// <summary>
        /// Registers handlers
        /// </summary>
        private WebSocketServerBuilder AddHandlers(ServiceLifetime lifetime, params Type[] assemblyTypes)
        {
            if (_services == null)
                throw new ArgumentNullException("ServiceCollection is not attached yet. Use AddBus method before adding handlers.");

            List<Type> types = _handler.Observer.RegisterWebSocketHandlers(() => _handler.ServiceProvider, assemblyTypes);
            foreach (Type type in types)
                RegisterHandler(lifetime, type);

            return this;
        }

        private void RegisterHandler(ServiceLifetime lifetime, Type serviceType)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Transient:
                    _services.AddTransient(serviceType);
                    break;

                case ServiceLifetime.Singleton:
                    _services.AddSingleton(serviceType);
                    break;

                case ServiceLifetime.Scoped:
                    _services.AddScoped(serviceType);
                    break;
            }
        }

        /// <summary>
        /// Gets message bus of websocket server
        /// </summary>
        /// <returns></returns>
        public WebSocketServerBuilder UseMSDI(IServiceCollection services)
        {
            if (!services.Any(x => x.ServiceType == typeof(IWebSocketServerBus)))
                services.AddSingleton<IWebSocketServerBus>(_handler);

            _services = services;

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