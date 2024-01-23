using System;
using Horse.Core;
using Horse.Server;
using Horse.WebSocket.Protocol;
using Microsoft.Extensions.DependencyInjection;

namespace Horse.WebSocket.Server;

/// <summary>
/// Server Implementations
/// </summary>
public static class ServerExtensions
{
    /// <summary>
    /// Initializes Horse WebSocket Server on HorseServer.
    /// This implementation requires second UseWebSockets call with IServiceProvider parameter.
    /// </summary>
    public static void AddWebSockets(this HorseServer server, IServiceCollection services, Action<WebSocketServerBuilder<IHorseWebSocket>> configureDelegate)
    {
        AddWebSockets<IHorseWebSocket>(server, services, configureDelegate);
    }


    /// <summary>
    /// Initializes Horse WebSocket Server on HorseServer.
    /// This implementation requires second UseWebSockets call with IServiceProvider parameter.
    /// </summary>
    public static void AddWebSockets<TClient>(this HorseServer server, IServiceCollection services, Action<WebSocketServerBuilder<TClient>> configureDelegate)
        where TClient : IHorseWebSocket
    {
        WebSocketServerBuilder<TClient> socketBuilder = new WebSocketServerBuilder<TClient>(services);

        configureDelegate(socketBuilder);
        HorseServer builtServer = server == null ? socketBuilder.Build() : socketBuilder.Build(server);

        services.AddSingleton<IHorseServer>(builtServer);
        services.AddSingleton(builtServer);
        services.AddSingleton(socketBuilder.Handler);
    }

    /// <summary>
    /// Initializes and starts websocket server handlers
    /// </summary>
    public static void UseWebSockets(this HorseServer server, IServiceProvider provider)
    {
        var handler = provider.GetService<ModelWsConnectionHandler>();
        handler.ServiceProvider = provider;
    }
}