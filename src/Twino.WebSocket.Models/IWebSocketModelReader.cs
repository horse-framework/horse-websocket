using System;
using System.Threading.Tasks;
using Twino.Protocols.WebSocket;

namespace Twino.WebSocket.Models
{
    public interface IWebSocketModelReader
    {
        object Read(WebSocketMessage message, Type modelType);
    }
}