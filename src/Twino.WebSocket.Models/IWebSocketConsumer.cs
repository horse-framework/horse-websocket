using System.Threading.Tasks;

namespace Twino.WebSocket.Models
{
    public interface IWebSocketConsumer<in TModel>
    {
        Task Consume(TModel model);
    }
}