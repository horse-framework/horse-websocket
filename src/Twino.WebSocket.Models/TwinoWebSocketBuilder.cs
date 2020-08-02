using System;
using System.Collections.Generic;
using Twino.Ioc;
using Twino.WebSocket.Models.Internal;

namespace Twino.WebSocket.Models
{
    /// <summary>
    /// Builder for websocket client connector and bus
    /// </summary>
    public class TwinoWebSocketBuilder
    {
        #region Fields

        private readonly List<Tuple<ImplementationType, Type, Type>> _individualConsumers = new List<Tuple<ImplementationType, Type, Type>>();
        private readonly List<Tuple<ImplementationType, Type>> _assembyConsumers = new List<Tuple<ImplementationType, Type>>();

        private WebSocketModelConnector _connector;
        private TimeSpan _reconnectInterval = TimeSpan.FromSeconds(1);

        private IWebSocketModelProvider _modelProvider;

        private Action<WebSocketModelConnector> _connected;
        private Action<WebSocketModelConnector> _disconnected;
        private Action<Exception> _error;

        private readonly List<string> _hosts = new List<string>();
        private readonly List<KeyValuePair<string, string>> _headers = new List<KeyValuePair<string, string>>();

        internal List<Tuple<ImplementationType, Type, Type>> IndividualConsumers => _individualConsumers;

        internal List<Tuple<ImplementationType, Type>> AssembyConsumers => _assembyConsumers;

        #endregion

        #region Connection

        /// <summary>
        /// Adds remote host
        /// </summary>
        public TwinoWebSocketBuilder AddHost(string hostname)
        {
            _hosts.Add(hostname);
            return this;
        }

        /// <summary>
        /// Adds a request header for HTTP protocol
        /// </summary>
        public TwinoWebSocketBuilder AddRequestHeader(string key, string value)
        {
            _headers.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        /// <summary>
        /// Sets reconnect interval for connector
        /// </summary>
        public TwinoWebSocketBuilder SetReconnectInterval(TimeSpan reconnectInterval)
        {
            _reconnectInterval = reconnectInterval;
            return this;
        }

        #endregion

        #region Events

        /// <summary>
        /// Action event is triggered when connection is established
        /// </summary>
        public TwinoWebSocketBuilder OnConnected(Action<WebSocketModelConnector> action)
        {
            _connected = action;
            return this;
        }

        /// <summary>
        /// Action event is triggered when disconnected from the server
        /// </summary>
        public TwinoWebSocketBuilder OnDisconnected(Action<WebSocketModelConnector> action)
        {
            _disconnected = action;
            return this;
        }

        /// <summary>
        /// Action event is triggered when an exception is thrown
        /// </summary>
        public TwinoWebSocketBuilder OnException(Action<Exception> action)
        {
            _error = action;
            return this;
        }

        #endregion

        #region Custom Processors

        /// <summary>
        /// Uses custom model provider
        /// </summary>
        public TwinoWebSocketBuilder UseCustomModelProvider<TProvider>()
            where TProvider : IWebSocketModelProvider, new()
        {
            _modelProvider = new TProvider();
            return this;
        }

        /// <summary>
        /// Uses custom model provider
        /// </summary>
        public TwinoWebSocketBuilder UseCustomModelProvider(IWebSocketModelProvider provider)
        {
            _modelProvider = provider;
            return this;
        }

        #endregion

        #region Handlers

        /// <summary>
        /// Registers new transient consumer
        /// </summary>
        public TwinoWebSocketBuilder AddTransientHandler<THandler, TModel>()
            where THandler : class, IWebSocketMessageHandler<TModel>
        {
            _individualConsumers.Add(new Tuple<ImplementationType, Type, Type>(ImplementationType.Transient, typeof(THandler), typeof(TModel)));
            return this;
        }

        /// <summary>
        /// Registers new scoped consumer
        /// </summary>
        public TwinoWebSocketBuilder AddScopedHandler<THandler, TModel>()
            where THandler : class, IWebSocketMessageHandler<TModel>
        {
            _individualConsumers.Add(new Tuple<ImplementationType, Type, Type>(ImplementationType.Scoped, typeof(THandler), typeof(TModel)));
            return this;
        }

        /// <summary>
        /// Registers new singleton consumer
        /// </summary>
        public TwinoWebSocketBuilder AddSingletonHandler<THandler, TModel>()
            where THandler : class, IWebSocketMessageHandler<TModel>
        {
            _individualConsumers.Add(new Tuple<ImplementationType, Type, Type>(ImplementationType.Singleton, typeof(THandler), typeof(TModel)));
            return this;
        }

        /// <summary>
        /// Registers all consumers types with transient lifetime in type assemblies
        /// </summary>
        public TwinoWebSocketBuilder AddTransientHandlers(params Type[] assemblyTypes)
        {
            foreach (Type type in assemblyTypes)
                _assembyConsumers.Add(new Tuple<ImplementationType, Type>(ImplementationType.Transient, type));

            return this;
        }

        /// <summary>
        /// Registers all consumers types with scoped lifetime in type assemblies
        /// </summary>
        public TwinoWebSocketBuilder AddScopedHandlers(params Type[] assemblyTypes)
        {
            foreach (Type type in assemblyTypes)
                _assembyConsumers.Add(new Tuple<ImplementationType, Type>(ImplementationType.Scoped, type));

            return this;
        }

        /// <summary>
        /// Registers all consumers types with singleton lifetime in type assemblies
        /// </summary>
        public TwinoWebSocketBuilder AddSingletonHandlers(params Type[] assemblyTypes)
        {
            foreach (Type type in assemblyTypes)
                _assembyConsumers.Add(new Tuple<ImplementationType, Type>(ImplementationType.Singleton, type));

            return this;
        }

        #endregion

        #region Build

        /// <summary>
        /// Creates, builds and returns connector
        /// </summary>
        public WebSocketModelConnector Build()
        {
            if (_connector != null)
                return _connector;

            _connector = new WebSocketModelConnector(_reconnectInterval);
            ConfigureConnector(_connector);
            return _connector;
        }

        /// <summary>
        /// Applies configurations on connector
        /// </summary>
        private void ConfigureConnector(WebSocketModelConnector connector)
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
            connector.Observer = new WebSocketMessageObserver(_modelProvider, _error);
        }

        /// <summary>
        /// Releases all resources
        /// </summary>
        internal void Dispose()
        {
            _connector = null;
            _connected = null;
            _disconnected = null;
            _error = null;
            _hosts.Clear();
            _headers.Clear();
        }

        #endregion
    }
}