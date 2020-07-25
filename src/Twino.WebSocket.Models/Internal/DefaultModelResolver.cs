using System;
using System.Collections.Generic;
using System.Text;
using Twino.Protocols.WebSocket;

namespace Twino.WebSocket.Models.Internal
{
    internal class DefaultModelResolver : IWebSocketModelResolver
    {
        private const byte COLON = (byte) '|';

        private readonly Dictionary<string, Type> _knownTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        public Type ResolveType(WebSocketMessage message)
        {
            message.Content.Position = 0;
            byte[] buffer = new byte[256];
            int count = message.Content.Read(buffer, 0, buffer.Length);
            if (count == 0)
                return null;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(buffer, 0, count);
            int i = s.IndexOf(COLON);
            if (i < 0)
                return null;

            string typeCode = Encoding.UTF8.GetString(buffer, 0, i);

            Type type;
            bool found = _knownTypes.TryGetValue(typeCode, out type);
            if (!found)
                return null;

            //leave it ready to start reading from beginning of the model itself
            message.Content.Position = i + 1;

            return type;
        }

        public void AddType(string typeCode, Type type)
        {
            _knownTypes.Add(typeCode, type);
        }
    }
}