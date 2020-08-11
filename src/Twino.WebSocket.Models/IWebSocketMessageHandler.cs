using System;
using System.Threading.Tasks;
using Twino.Protocols.WebSocket;

namespace Twino.WebSocket.Models
{
    /// <summary>
    /// Websocket message handler implementation for websocket messages
    /// </summary>
    public interface IWebSocketMessageHandler<in TModel>
    {
        /// <summary>
        /// Handles websocket message
        /// </summary>
        Task Handle(TModel model, WebSocketMessage message, ITwinoWebSocket client);

        /// <summary>
        /// Triggered when an error occured in handle method
        /// </summary>
        Task OnError(Exception exception, TModel model, WebSocketMessage message, ITwinoWebSocket client);
    }
}