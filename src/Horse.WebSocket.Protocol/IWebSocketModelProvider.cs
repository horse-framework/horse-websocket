using System;

namespace Horse.WebSocket.Protocol;

/// <summary>
/// Generates code from a type.
/// Type codes are used for resolving and writing models.
/// </summary>
public interface IWebSocketModelProvider
{
    /// <summary>
    /// True, if provider reads and writes data with binary OpCode on websocket connection.
    /// </summary>
    bool Binary { get; }
    
    /// <summary>
    /// Resolves model type from websocket message
    /// </summary>
    Type Resolve(WebSocketMessage message);

    /// <summary>
    /// Registers new type to resolver with a code
    /// </summary>
    void Register(Type type);

    /// <summary>
    /// Gets a model from websocket message
    /// </summary>
    object Get(WebSocketMessage message, Type modelType);

    /// <summary>
    /// Writes a model to websocket message
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    WebSocketMessage Write(object model);

    /// <summary>
    /// Writes a model with customized code to websocket message
    /// </summary>
    /// <param name="customCode">Customized code. This value overrides default code for specified type</param>
    /// <param name="model"></param>
    /// <returns></returns>
    WebSocketMessage Write(string customCode, object model);
}