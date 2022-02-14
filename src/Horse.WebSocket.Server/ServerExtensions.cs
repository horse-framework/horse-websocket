using System;
using Horse.Core;
using Horse.Core.Protocols;
using Horse.Protocols.Http;
using Horse.WebSocket.Protocol;
using Microsoft.Extensions.DependencyInjection;

namespace Horse.WebSocket.Server;

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
        ModelWsConnectionHandler bus = (ModelWsConnectionHandler) provider.GetService<IWebSocketServerBus>();
        bus.ServiceProvider = provider;
        return server;
    }

    /// <summary>
    /// Uses WebSocket Protocol and accepts HTTP connections which comes with "Upgrade: websocket" header data
    /// </summary>
    public static IHorseServer UseWebSockets(this IHorseServer server,
        IProtocolConnectionHandler<WsServerSocket, WebSocketMessage> handler)
    {
        return UseWebSockets(server, handler, HttpOptions.CreateDefault());
    }

    /// <summary>
    /// Uses WebSocket Protocol and accepts HTTP connections which comes with "Upgrade: websocket" header data
    /// </summary>
    public static IHorseServer UseWebSockets(this IHorseServer server,
        WebSocketMessageRecievedHandler handlerAction)
    {
        return UseWebSockets(server, new MethodWebSocketConnectionHandler(handlerAction), HttpOptions.CreateDefault());
    }

    /// <summary>
    /// Uses WebSocket Protocol and accepts HTTP connections which comes with "Upgrade: websocket" header data
    /// </summary>
    public static IHorseServer UseWebSockets(this IHorseServer server,
        WebSocketMessageRecievedHandler handlerAction,
        HttpOptions options)
    {
        return UseWebSockets(server, new MethodWebSocketConnectionHandler(handlerAction), options);
    }

    /// <summary>
    /// Uses WebSocket Protocol and accepts HTTP connections which comes with "Upgrade: websocket" header data
    /// </summary>
    public static IHorseServer UseWebSockets(this IHorseServer server,
        IProtocolConnectionHandler<WsServerSocket, WebSocketMessage> handler,
        HttpOptions options)
    {
        //we need http protocol is added
        IHorseProtocol http = server.FindProtocol("http");
        if (http == null)
        {
            HorseHttpProtocol httpProtocol = new HorseHttpProtocol(server, new WebSocketHttpHandler(), options);
            server.UseProtocol(httpProtocol);
        }

        HorseWebSocketProtocol protocol = new HorseWebSocketProtocol(server, handler);
        server.UseProtocol(protocol);
        return server;
    }

    /// <summary>
    /// Uses WebSocket Protocol and accepts HTTP connections which comes with "Upgrade: websocket" header data
    /// </summary>
    public static IHorseServer UseWebSockets(this IHorseServer server,
        WebSocketConnectedHandler connectedAction,
        WebSocketMessageRecievedHandler messageAction)
    {
        return UseWebSockets(server,
            new MethodWebSocketConnectionHandler(connectedAction, null, messageAction),
            HttpOptions.CreateDefault());
    }

    /// <summary>
    /// Uses WebSocket Protocol and accepts HTTP connections which comes with "Upgrade: websocket" header data
    /// </summary>
    public static IHorseServer UseWebSockets(this IHorseServer server,
        WebSocketConnectedHandler connectedAction,
        WebSocketReadyHandler readyAction,
        WebSocketMessageRecievedHandler messageAction)
    {
        return UseWebSockets(server,
            new MethodWebSocketConnectionHandler(connectedAction, readyAction, messageAction),
            HttpOptions.CreateDefault());
    }

    /// <summary>
    /// Uses WebSocket Protocol and accepts HTTP connections which comes with "Upgrade: websocket" header data
    /// </summary>
    public static IHorseServer UseWebSockets(this IHorseServer server,
        WebSocketReadyHandler readyAction,
        WebSocketMessageRecievedHandler messageAction)
    {
        return UseWebSockets(server,
            new MethodWebSocketConnectionHandler(null, readyAction, messageAction),
            HttpOptions.CreateDefault());
    }
}