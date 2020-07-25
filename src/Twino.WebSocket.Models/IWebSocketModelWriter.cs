using System;
using System.Threading.Tasks;
using Twino.Protocols.WebSocket;

namespace Twino.WebSocket.Models
{
    public interface IWebSocketModelWriter
    {
        WebSocketMessage Write(object model, Type modelType);
    }
}