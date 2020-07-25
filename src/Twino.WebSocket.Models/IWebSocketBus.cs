using System.Threading.Tasks;

namespace Twino.WebSocket.Models
{
    public interface IWebSocketBus
    {
        Task<bool> Send<TModel>(TModel model);
    }
}