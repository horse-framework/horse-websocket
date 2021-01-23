using System;
using Horse.Protocols.WebSocket;

namespace Horse.WebSocket.Models
{
    /// <summary>
    /// Generates code from a type.
    /// Type codes are used for resolving and writing models.
    /// </summary>
    public interface IWebSocketModelProvider
    {
        /// <summary>
        /// Resolves model type from websocket message
        /// </summary>
        Type Resolve(WebSocketMessage message);

        /// <summary>
        /// Gets code of the type
        /// </summary>
        string GetCode(Type type);

        /// <summary>
        /// Gets registered type
        /// </summary>
        Type GetType(string code);
        
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
    }
}