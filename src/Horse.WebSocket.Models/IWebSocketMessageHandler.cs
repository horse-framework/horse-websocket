using System;
using System.Threading.Tasks;
using Horse.Protocols.WebSocket;

namespace Horse.WebSocket.Models
{
    /// <summary>
    /// Websocket message handler implementation for websocket messages
    /// </summary>
    public interface IWebSocketMessageHandler<in TModel>
    {
        /// <summary>
        /// Handles websocket message
        /// </summary>
        Task Handle(TModel model, WebSocketMessage message, IHorseWebSocket client);

        /// <summary>
        /// Triggered when an error occured in handle method
        /// </summary>
        Task OnError(Exception exception, TModel model, WebSocketMessage message, IHorseWebSocket client);
    }
}