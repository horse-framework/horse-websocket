using System;
using System.Collections.Generic;
using Twino.Client.WebSocket.Connectors;
using Twino.WebSocket.Models.Internal;

namespace Twino.WebSocket.Models
{
    public class TwinoWebSocketBuilder
    {
        #region Fields

        private WebSocketModelConnector _connector;
        private TimeSpan _reconnectInterval = TimeSpan.FromSeconds(1);

        private IWebSocketModelResolver _resolver;
        private IWebSocketModelWriter _writer;
        private IWebSocketModelReader _reader;

        private Action<WebSocketModelConnector> _connected;
        private Action<WebSocketModelConnector> _disconnected;
        private Action<Exception> _error;

        private readonly List<string> _hosts = new List<string>();
        private readonly List<KeyValuePair<string, string>> _headers = new List<KeyValuePair<string, string>>();

        #endregion

        #region Connection

        public TwinoWebSocketBuilder AddHost(string hostname)
        {
            _hosts.Add(hostname);
            return this;
        }

        public TwinoWebSocketBuilder AddRequestHeader(string key, string value)
        {
            _headers.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        public TwinoWebSocketBuilder SetReconnectInterval(TimeSpan reconnectInterval)
        {
            _reconnectInterval = reconnectInterval;
            return this;
        }

        #endregion

        #region Events

        public TwinoWebSocketBuilder OnConnected(Action<WsStickyConnector> action)
        {
            _connected = action;
            return this;
        }

        public TwinoWebSocketBuilder OnDisconnected(Action<WsStickyConnector> action)
        {
            _disconnected = action;
            return this;
        }

        public TwinoWebSocketBuilder OnException(Action<Exception> action)
        {
            _error = action;
            return this;
        }

        #endregion

        #region Custom Processors

        public TwinoWebSocketBuilder UseCustomResolver<TResolver>()
            where TResolver : IWebSocketModelResolver, new()
        {
            _resolver = new TResolver();
            return this;
        }

        public TwinoWebSocketBuilder UseCustomReaderWriter<TReader, TWriter>()
            where TReader : IWebSocketModelReader, new()
            where TWriter : IWebSocketModelWriter, new()
        {
            _reader = new TReader();
            _writer = new TWriter();
            return this;
        }

        #endregion

        #region Build

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

            connector.ModelResolver = _resolver ?? new DefaultModelResolver();
            connector.ModelWriter = _writer ?? new DefaultModelWriter();
            connector.ModelReader = _reader ?? new DefaultModelReader();
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