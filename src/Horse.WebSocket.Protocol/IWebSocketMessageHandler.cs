using System;
using System.Threading.Tasks;

namespace Horse.WebSocket.Protocol;

/// <summary>
/// Websocket message handler implementation for websocket messages
/// </summary>
public interface IWebSocketMessageHandler<in TModel> : IWebSocketMessageHandler<TModel, IHorseWebSocket>
{
}

/// <summary>
/// Websocket message handler implementation for websocket messages
/// </summary>
public interface IWebSocketMessageHandler<in TModel, in TClient> where TClient : IHorseWebSocket
{
    /// <summary>
    /// Handles websocket message
    /// </summary>
    ValueTask Handle(TModel model, WebSocketMessage message, TClient client);

    /// <summary>
    /// Triggered when an error occured in handle method
    /// </summary>
    ValueTask OnError(Exception exception, TModel model, WebSocketMessage message, TClient client)
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Triggered when an authentication failed
    /// </summary>
    ValueTask OnUnauthenticated(TModel model, WebSocketMessage message, TClient client)
    {
        return ValueTask.CompletedTask;
    }
}