using System;
using System.Threading.Tasks;

namespace Horse.WebSocket.Protocol.Security;

/// <summary>
/// Authenticator implementation for websocket
/// </summary>
public interface IWebSocketAuthenticator
{
    /// <summary>
    /// Authenticates user for specified message
    /// </summary>
    Task<bool> Authenticate(WsServerSocket client, WebSocketMessage message, Type modelType, Type handlerType);
}