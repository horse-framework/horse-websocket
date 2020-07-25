using System;
using System.Threading.Tasks;
using Twino.Protocols.WebSocket;

namespace Twino.WebSocket.Models
{
    public interface IWebSocketModelResolver
    {
        Type ResolveType(WebSocketMessage message);

        void AddType(string typeCode, Type type);
    }
}