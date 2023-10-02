using System;
using Horse.Core;
using Horse.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Horse.WebSocket.Server;

/// <summary>
/// Server Implementations
/// </summary>
public static class ServerExtensions
{
    /// <summary>
    /// Initializes Horse WebSocket Server on HorseServer
    /// </summary>
    public static void AddWebSockets(this HorseServer server, IServiceCollection services, Action<WebSocketServerBuilder> configureDelegate)
    {
        WebSocketServerBuilder socketBuilder = new WebSocketServerBuilder();

        configureDelegate(socketBuilder);
        HorseServer builtServer = server == null ? socketBuilder.Build() : socketBuilder.Build(server);
        socketBuilder.UseMSDI(services);

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