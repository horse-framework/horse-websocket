using System;
using System.Collections.Generic;
using Horse.WebSocket.Models.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Horse.WebSocket.Models
{
    /// <summary>
    /// Builder for websocket client connector and bus
    /// </summary>
    public class HorseWebSocketBuilder<TIdentifier> : HorseWebSocketBuilder
    {
        /// <summary>
        /// Creates, builds and returns connector
        /// </summary>
        public override WebSocketModelConnector Build()
        {
            if (Connector != null)
                return Connector;

            Connector = new WebSocketModelConnector<TIdentifier>(ReconnectInterval);
            ConfigureConnector(Connector);
            return Connector;
        }
    }

    /// <summary>
    /// Builder for websocket client connector and bus
    /// </summary>
    public class HorseWebSocketBuilder
    {
        #region Fields

        private readonly List<Tuple<ServiceLifetime, Type, Type>> _individualConsumers = new List<Tuple<ServiceLifetime, Type, Type>>();
        private readonly List<Tuple<ServiceLifetime, Type>> _assembyConsumers = new List<Tuple<ServiceLifetime, Type>>();

        /// <summary>
        /// Connector object of the builder
        /// </summary>
        protected internal WebSocketModelConnector Connector { get; set; }
        
        /// <summary>
        /// Connector reconnection delay
        /// </summary>
        protected internal TimeSpan ReconnectInterval { get; private set; } = TimeSpan.FromSeconds(1);

        private IWebSocketModelProvider _modelProvider;

        private Action<WebSocketModelConnector> _connected;
        private Action<WebSocketModelConnector> _disconnected;
        private Action<Exception> _error;

        private readonly List<string> _hosts = new List<string>();
        private readonly List<KeyValuePair<string, string>> _headers = new List<KeyValuePair<string, string>>();

        internal List<Tuple<ServiceLifetime, Type, Type>> IndividualConsumers => _individualConsumers;

        internal List<Tuple<ServiceLifetime, Type>> AssembyConsumers => _assembyConsumers;

        #endregion

        #region Connection

        /// <summary>
        /// Adds remote host
        /// </summary>
        public HorseWebSocketBuilder AddHost(string hostname)
        {
            _hosts.Add(hostname);
            return this;
        }

        /// <summary>
        /// Adds a request header for HTTP protocol
        /// </summary>
        public HorseWebSocketBuilder AddRequestHeader(string key, string value)
        {
            _headers.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        /// <summary>
        /// Sets reconnect interval for connector
        /// </summary>
        public HorseWebSocketBuilder SetReconnectInterval(TimeSpan reconnectInterval)
        {
            ReconnectInterval = reconnectInterval;
            return this;
        }

        #endregion

        #region Events

        /// <summary>
        /// Action event is triggered when connection is established
        /// </summary>
        public HorseWebSocketBuilder OnConnected(Action<WebSocketModelConnector> action)
        {
            _connected = action;
            return this;
        }

        /// <summary>
        /// Action event is triggered when disconnected from the server
        /// </summary>
        public HorseWebSocketBuilder OnDisconnected(Action<WebSocketModelConnector> action)
        {
            _disconnected = action;
            return this;
        }

        /// <summary>
        /// Action event is triggered when an exception is thrown
        /// </summary>
        public HorseWebSocketBuilder OnException(Action<Exception> action)
        {
            _error = action;
            return this;
        }

        #endregion

        #region Custom Processors

        /// <summary>
        /// Uses custom model provider
        /// </summary>
        public HorseWebSocketBuilder UseCustomModelProvider<TProvider>()
            where TProvider : IWebSocketModelProvider, new()
        {
            _modelProvider = new TProvider();
            return this;
        }

        /// <summary>
        /// Uses custom model provider
        /// </summary>
        public HorseWebSocketBuilder UseCustomModelProvider(IWebSocketModelProvider provider)
        {
            _modelProvider = provider;
            return this;
        }

        /// <summary>
        /// Uses payload model provider.
        /// Models are sent in payload property in JSON model { type: "model-type", payload: your_model }
        /// </summary>
        public HorseWebSocketBuilder UsePayloadModelProvider()
        {
            _modelProvider = new PayloadModelProvider();
            return this;
        }

        #endregion

        #region Handlers

        /// <summary>
        /// Registers new transient consumer
        /// </summary>
        public HorseWebSocketBuilder AddTransientHandler<THandler, TModel>()
            where THandler : class, IWebSocketMessageHandler<TModel>
        {
            _individualConsumers.Add(new Tuple<ServiceLifetime, Type, Type>(ServiceLifetime.Transient, typeof(THandler), typeof(TModel)));
            return this;
        }

        /// <summary>
        /// Registers new scoped consumer
        /// </summary>
        public HorseWebSocketBuilder AddScopedHandler<THandler, TModel>()
            where THandler : class, IWebSocketMessageHandler<TModel>
        {
            _individualConsumers.Add(new Tuple<ServiceLifetime, Type, Type>(ServiceLifetime.Scoped, typeof(THandler), typeof(TModel)));
            return this;
        }

        /// <summary>
        /// Registers new singleton consumer
        /// </summary>
        public HorseWebSocketBuilder AddSingletonHandler<THandler, TModel>()
            where THandler : class, IWebSocketMessageHandler<TModel>
        {
            _individualConsumers.Add(new Tuple<ServiceLifetime, Type, Type>(ServiceLifetime.Singleton, typeof(THandler), typeof(TModel)));
            return this;
        }

        /// <summary>
        /// Registers all consumers types with transient lifetime in type assemblies
        /// </summary>
        public HorseWebSocketBuilder AddTransientHandlers(params Type[] assemblyTypes)
        {
            foreach (Type type in assemblyTypes)
                _assembyConsumers.Add(new Tuple<ServiceLifetime, Type>(ServiceLifetime.Transient, type));

            return this;
        }

        /// <summary>
        /// Registers all consumers types with scoped lifetime in type assemblies
        /// </summary>
        public HorseWebSocketBuilder AddScopedHandlers(params Type[] assemblyTypes)
        {
            foreach (Type type in assemblyTypes)
                _assembyConsumers.Add(new Tuple<ServiceLifetime, Type>(ServiceLifetime.Scoped, type));

            return this;
        }

        /// <summary>
        /// Registers all consumers types with singleton lifetime in type assemblies
        /// </summary>
        public HorseWebSocketBuilder AddSingletonHandlers(params Type[] assemblyTypes)
        {
            foreach (Type type in assemblyTypes)
                _assembyConsumers.Add(new Tuple<ServiceLifetime, Type>(ServiceLifetime.Singleton, type));

            return this;
        }

        #endregion

        #region Build

        /// <summary>
        /// Creates, builds and returns connector
        /// </summary>
        public virtual WebSocketModelConnector Build()
        {
            if (Connector != null)
                return Connector;

            Connector = new WebSocketModelConnector(ReconnectInterval);
            ConfigureConnector(Connector);
            return Connector;
        }

        /// <summary>
        /// Applies configurations on connector
        /// </summary>
        protected void ConfigureConnector(WebSocketModelConnector connector)
        {
            foreach (string host in _hosts)
                connector.AddHost(host);

            foreach (KeyValuePair<string, string> pair in _headers)
                connector.AddProperty(pair.Key, pair.Value);

            if (_connected != null)
                connector.Connected += new ConnectionEventMapper(connector, _connected).Action;

            if (_disconnected != null)
                connector.Disconnected += new ConnectionEventMapper(connector, _disconnected).Action;

            if (_error != null)
                connector.ExceptionThrown += new ExceptionEventMapper(connector, _error).Action;

            connector.ModelProvider = _modelProvider ?? new WebSocketModelProvider();
            connector.Observer = new WebSocketMessageObserver(connector.ModelProvider, _error);
        }

        /// <summary>
        /// Releases all resources
        /// </summary>
        internal void Dispose()
        {
            Connector = null;
            _connected = null;
            _disconnected = null;
            _error = null;
            _hosts.Clear();
            _headers.Clear();
        }

        #endregion
    }
}