using System;
using Horse.Core;
using Horse.Core.Protocols;
using Horse.Protocols.Http;
using Horse.Protocols.WebSocket;

namespace Horse.WebSocket.Models
{
    /// <summary>
    /// WebSocket extensions for server
    /// </summary>
    public static class ServerExtensions
    {
        /// <summary>
        /// Uses websocket protocol
        /// </summary>
        public static IHorseServer AddWebSockets(this IHorseServer server, Action<WebSocketServerBuilder> cfg)
        {
            return AddWebSockets(server, HttpOptions.CreateDefault(), cfg);
        }

        /// <summary>
        /// Uses websocket protocol
        /// </summary>
        public static IHorseServer AddWebSockets(this IHorseServer server, HttpOptions options, Action<WebSocketServerBuilder> cfg)
        {
            //we need http protocol is added
            IHorseProtocol http = server.FindProtocol("http");
            if (http == null)
            {
                HorseHttpProtocol httpProtocol = new HorseHttpProtocol(server, new WebSocketHttpHandler(), options);
                server.UseProtocol(httpProtocol);
            }

            WebSocketServerBuilder builder = new WebSocketServerBuilder();
            cfg(builder);
            ModelWsConnectionHandler handler = builder.Build();

            HorseWebSocketProtocol protocol = new HorseWebSocketProtocol(server, handler);
            server.UseProtocol(protocol);
            return server;
        }

        /// <summary>
        /// Uses websockets with service provider
        /// </summary>
        public static IHorseServer UseWebSockets(this IHorseServer server, IServiceProvider provider)
        {
            ModelWsConnectionHandler bus = (ModelWsConnectionHandler) provider.GetService(typeof(IWebSocketServerBus));
            bus.ServiceProvider = provider;
            return server;
        }
    }
}