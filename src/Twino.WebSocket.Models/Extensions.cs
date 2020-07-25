using System;
using Twino.Ioc;

namespace Twino.WebSocket.Models
{
    public static class Extensions
    {
        public static IServiceContainer UseTwinoWebSockets(this IServiceContainer services, Action<TwinoWebSocketBuilder> config)
        {
            throw new NotImplementedException();
        }
    }
}