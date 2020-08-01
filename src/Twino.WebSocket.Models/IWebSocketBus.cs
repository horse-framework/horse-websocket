using System.Threading.Tasks;

namespace Twino.WebSocket.Models
{
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