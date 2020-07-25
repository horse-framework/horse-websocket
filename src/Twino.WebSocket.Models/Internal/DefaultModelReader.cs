using System;
using System.IO;
using Twino.Protocols.WebSocket;

namespace Twino.WebSocket.Models.Internal
{
    internal class DefaultModelReader : IWebSocketModelReader
    {
        public object Read(WebSocketMessage message, Type modelType)
        {
            StreamReader reader = new StreamReader(message.Content);
            string serialized = reader.ReadToEnd();
            return Newtonsoft.Json.JsonConvert.DeserializeObject(serialized, modelType);
        }
    }
}