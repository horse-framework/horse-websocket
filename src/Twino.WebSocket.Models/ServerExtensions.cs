using System;
using Twino.Core;
using Twino.Core.Protocols;
using Twino.Protocols.Http;
using Twino.Protocols.WebSocket;

namespace Twino.WebSocket.Models
{
    /// <summary>
    /// WebSocket extensions for server
    /// </summary>
    public static class ServerExtensions
    {
        /// <summary>
        /// Uses websocket protocol
        /// </summary>
        public static ITwinoServer UseWebSockets(this ITwinoServer server, Action<WebSocketServerBuilder> cfg)
        {
            return UseWebSockets(server, HttpOptions.CreateDefault(), cfg);
        }

        /// <summary>
        /// Uses websocket protocol
        /// </summary>
        public static ITwinoServer UseWebSockets(this ITwinoServer server, HttpOptions options, Action<WebSocketServerBuilder> cfg)
        {
            //we need http protocol is added
            ITwinoProtocol http = server.FindProtocol("http");
            if (http == null)
            {
                TwinoHttpProtocol httpProtocol = new TwinoHttpProtocol(server, new WebSocketHttpHandler(), options);
                server.UseProtocol(httpProtocol);
            }

            WebSocketServerBuilder builder = new WebSocketServerBuilder();
            cfg(builder);
            ModelWsConnectionHandler handler = builder.Build();

            TwinoWebSocketProtocol protocol = new TwinoWebSocketProtocol(server, handler);
            server.UseProtocol(protocol);
            return server;
        }
    }
}