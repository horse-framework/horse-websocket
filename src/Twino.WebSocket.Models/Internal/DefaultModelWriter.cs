using System;
using System.Collections.Generic;
using System.Reflection;
using Twino.Protocols.WebSocket;

namespace Twino.WebSocket.Models.Internal
{
    internal class DefaultModelWriter : IWebSocketModelWriter
    {
        private readonly Dictionary<Type, string> _typeCodes = new Dictionary<Type, string>();

        private string FindTypeCode(Type type)
        {
            string code;
            bool found = _typeCodes.TryGetValue(type, out code);
            if (found)
                return code;

            ModelTypeAttribute attribute = type.GetCustomAttribute<ModelTypeAttribute>(false);
            code = attribute != null ? attribute.TypeName : type.Name;

            _typeCodes.Add(type, code);
            return code;
        }

        internal void AddType(Type type, string code)
        {
            _typeCodes.Add(type, code);
        }

        public WebSocketMessage Write(object model, Type modelType)
        {
            string code = FindTypeCode(modelType);
            string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(modelType);

            WebSocketMessage message = WebSocketMessage.FromString(code + "|" + serialized);
            return message;
        }
    }
}