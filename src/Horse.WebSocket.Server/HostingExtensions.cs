using System;
using Horse.Core;
using Horse.Core.Protocols;
using Horse.Protocols.Http;
using Horse.Server;
using Horse.WebSocket.Protocol;
using Horse.WebSocket.Protocol.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Horse.WebSocket.Server;

/// <summary>
/// Microsoft Hosting extensions for creating WebSocket Server
/// </summary>
public static class HostingExtensions
{
    /// <summary>
    /// Creates new Horse Server and implements WebSocket Server into it
    /// </summary>
    /// <param name="builder">Host Builder</param>
    /// <param name="configureDelegate">Configuration delegate</param>
    /// <returns></returns>
    public static IHostBuilder UseHorseWebSocketServer(this IHostBuilder builder, Action<HostBuilderContext, WebSocketServerBuilder<IHorseWebSocket>> configureDelegate)
    {
        return UseHorseWebSocketServer(builder, null, configureDelegate);
    }

    /// <summary>
    /// Creates new Horse Server and implements WebSocket Server into it
    /// </summary>
    /// <param name="builder">Host Builder</param>
    /// <param name="configureDelegate">Configuration delegate</param>
    /// <returns></returns>
    public static IHostBuilder UseHorseWebSocketServer<TClient>(this IHostBuilder builder, Action<HostBuilderContext, WebSocketServerBuilder<TClient>> configureDelegate)
        where TClient : IHorseWebSocket
    {
        return UseHorseWebSocketServer(builder, null, configureDelegate);
    }


    /// <summary>
    /// Creates new Horse WebSocket Server
    /// </summary>
    /// <param name="builder">Host Builder</param>
    /// <param name="server">Custom server implementation</param>
    /// <param name="configureDelegate">Configuration delegate</param>
    /// <returns></returns>
    public static IHostBuilder UseHorseWebSocketServer(this IHostBuilder builder, HorseServer server, Action<HostBuilderContext, WebSocketServerBuilder<IHorseWebSocket>> configureDelegate)
    {
        return UseHorseWebSocketServer<IHorseWebSocket>(builder, server, configureDelegate);
    }

    /// <summary>
    /// Creates new Horse WebSocket Server
    /// </summary>
    /// <param name="builder">Host Builder</param>
    /// <param name="server">Custom server implementation</param>
    /// <param name="configureDelegate">Configuration delegate</param>
    /// <returns></returns>
    public static IHostBuilder UseHorseWebSocketServer<TClient>(this IHostBuilder builder, HorseServer server, Action<HostBuilderContext, WebSocketServerBuilder<TClient>> configureDelegate)
        where TClient : IHorseWebSocket
    {
        HorseServer builtServer;

        builder.ConfigureServices((context, services) =>
        {
            WebSocketServerBuilder<TClient> socketBuilder = new WebSocketServerBuilder<TClient>(services);
            configureDelegate(context, socketBuilder);
            builtServer = server == null ? socketBuilder.Build() : socketBuilder.Build(server);

            services.AddSingleton<IHorseServer>(builtServer);
            services.AddSingleton(builtServer);

            services.AddHostedService(provider =>
            {
                socketBuilder.Handler.ServiceProvider = provider;
                WebSocketRunnerService hostedService = new WebSocketRunnerService(builtServer, provider, socketBuilder.Port);
                return hostedService;
            });
        });

        return builder;
    }

    /// <summary>
    /// Creates new Horse WebSocket Server
    /// </summary>
    /// <param name="builder">Host Builder</param>
    /// <param name="server">Custom server implementation</param>
    /// <param name="configureDelegate">Configuration delegate</param>
    /// <returns></returns>
    public static IHostBuilder UseHorseWebSocketServer(this IHostBuilder builder, HorseServer server, Action<HostBuilderContext, IServiceCollection, WebSocketServerBuilder<IHorseWebSocket>> configureDelegate)
    {
        return UseHorseWebSocketServer<IHorseWebSocket>(builder, server, configureDelegate);
    }

    /// <summary>
    /// Creates new Horse WebSocket Server
    /// </summary>
    /// <param name="builder">Host Builder</param>
    /// <param name="server">Custom server implementation</param>
    /// <param name="configureDelegate">Configuration delegate</param>
    /// <returns></returns>
    public static IHostBuilder UseHorseWebSocketServer<TClient>(this IHostBuilder builder, HorseServer server, Action<HostBuilderContext, IServiceCollection, WebSocketServerBuilder<TClient>> configureDelegate)
        where TClient : IHorseWebSocket
    {
        HorseServer builtServer;

        builder.ConfigureServices((context, services) =>
        {
            WebSocketServerBuilder<TClient> socketBuilder = new WebSocketServerBuilder<TClient>(services);
            configureDelegate(context, services, socketBuilder);

            builtServer = server == null ? socketBuilder.Build() : socketBuilder.Build(server);

            services.AddSingleton<IHorseServer>(builtServer);
            services.AddSingleton(builtServer);

            services.AddHostedService(provider =>
            {
                socketBuilder.Handler.ServiceProvider = provider;
                WebSocketRunnerService hostedService = new WebSocketRunnerService(builtServer, provider, socketBuilder.Port);
                return hostedService;
            });
        });

        return builder;
    }

    /// <summary>
    /// Creates new Horse WebSocket Server
    /// </summary>
    /// <param name="builder">Host Builder</param>
    /// <param name="port">Server Port</param>
    /// <param name="handler"></param>
    /// <param name="encryptor">Message encryptor (optional)</param>
    /// <returns></returns>
    public static IHostBuilder UseHorseWebSocketServer(this IHostBuilder builder, int port,
        IProtocolConnectionHandler<WsServerSocket, WebSocketMessage> handler, IMessageEncryptor encryptor = null)
    {
        return UseHorseWebSocketServer(builder, new HorseServer(), port, handler, encryptor);
    }

    /// <summary>
    /// Creates new Horse WebSocket Server
    /// </summary>
    /// <param name="builder">Host Builder</param>
    /// <param name="server"></param>
    /// <param name="port">Server Port</param>
    /// <param name="handler"></param>
    /// <param name="encryptor">Message encryptor (optional)</param>
    /// <returns></returns>
    public static IHostBuilder UseHorseWebSocketServer(this IHostBuilder builder, HorseServer server, int port,
        IProtocolConnectionHandler<WsServerSocket, WebSocketMessage> handler, IMessageEncryptor encryptor = null)
    {
        IHorseProtocol http = server.FindProtocol("http");
        if (http == null)
        {
            HorseHttpProtocol httpProtocol = new HorseHttpProtocol(server, new WebSocketHttpHandler(), HttpOptions.CreateDefault());
            server.UseProtocol(httpProtocol);
        }

        HorseWebSocketProtocol protocol = new HorseWebSocketProtocol(server, handler);
        protocol.Encryptor = encryptor;
        server.UseProtocol(protocol);

        builder.ConfigureServices((context, services) =>
        {
            services.AddSingleton<IHorseServer>(server);
            services.AddSingleton(server);

            services.AddHostedService(provider =>
            {
                WebSocketRunnerService hostedService = new WebSocketRunnerService(server, provider, port);
                return hostedService;
            });
        });

        return builder;
    }

    /// <summary>
    /// Creates new Horse WebSocket Server
    /// </summary>
    /// <param name="builder">Host Builder</param>
    /// <param name="port">Server Port</param>
    /// <param name="connectedAction">Action to execute when a client connected</param>
    /// <param name="readyAction">Action to execute when a client is ready to send and receive messages</param>
    /// <param name="messageAction">Action to execute when client sends a message to the server</param>
    /// <param name="encryptor">Message encryptor (optional)</param>
    /// <returns></returns>
    public static IHostBuilder UseHorseWebSocketServer(this IHostBuilder builder, int port,
        WebSocketConnectedHandler connectedAction,
        WebSocketReadyHandler readyAction,
        WebSocketMessageRecievedHandler messageAction,
        IMessageEncryptor encryptor = null)
    {
        return UseHorseWebSocketServer(builder, new HorseServer(), port, connectedAction, readyAction, messageAction, encryptor);
    }

    /// <summary>
    /// Creates new Horse WebSocket Server
    /// </summary>
    /// <param name="builder">Host Builder</param>
    /// <param name="server">Horse Server</param>
    /// <param name="port">Server Port</param>
    /// <param name="connectedAction">Action to execute when a client connected</param>
    /// <param name="readyAction">Action to execute when a client is ready to send and receive messages</param>
    /// <param name="messageAction">Action to execute when client sends a message to the server</param>
    /// <param name="encryptor">Message encryptor (optional)</param>
    /// <returns></returns>
    public static IHostBuilder UseHorseWebSocketServer(this IHostBuilder builder,
        HorseServer server, int port,
        WebSocketConnectedHandler connectedAction,
        WebSocketReadyHandler readyAction,
        WebSocketMessageRecievedHandler messageAction,
        IMessageEncryptor encryptor = null)
    {
        IHorseProtocol http = server.FindProtocol("http");
        if (http == null)
        {
            HorseHttpProtocol httpProtocol = new HorseHttpProtocol(server, new WebSocketHttpHandler(), HttpOptions.CreateDefault());
            server.UseProtocol(httpProtocol);
        }

        var handler = new MethodWebSocketConnectionHandler(connectedAction, readyAction, messageAction);
        HorseWebSocketProtocol protocol = new HorseWebSocketProtocol(server, handler);
        protocol.Encryptor = encryptor;
        server.UseProtocol(protocol);

        builder.ConfigureServices((context, services) =>
        {
            services.AddSingleton<IHorseServer>(server);
            services.AddSingleton(server);

            services.AddHostedService(provider =>
            {
                WebSocketRunnerService hostedService = new WebSocketRunnerService(server, provider, port);
                return hostedService;
            });
        });

        return builder;
    }
}