using System.Threading.Tasks;

namespace Twino.WebSocket.Models
{
    /// <summary>
    /// WebSocket Message Bus with unique identifier to register multiple client buses into same ioc container.
    /// If you are using only one bus in a container, use non-generic instead.
    /// </summary>
    public interface IWebSocketBus<TIdentifier> : IWebSocketBus
    {
    }
    
    /// <summary>
    /// WebSocket Message Bus
    /// </summary>
    public interface IWebSocketBus
    {
        /// <summary>
        /// Sends a message over websocket
        /// </summary>
        Task<bool> SendAsync<TModel>(TModel model);
    }
}