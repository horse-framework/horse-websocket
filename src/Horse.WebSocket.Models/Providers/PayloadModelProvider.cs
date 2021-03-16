using System;
using System.Collections.Generic;
using System.Reflection;
using Horse.Protocols.WebSocket;

namespace Horse.WebSocket.Models.Providers
{
    /// <summary>
    /// Payload provider by class name.
    /// Serialize data seems like; {type:"ModelA",payload:{name:"foo",no:123}}
    /// </summary>
    public class PayloadModelProvider : IWebSocketModelProvider
    {
        /// <summary>
        /// For getting codes by type
        /// </summary>
        private readonly Dictionary<Type, string> _typeCodes = new();

        /// <summary>
        /// For getting type by code
        /// </summary>
        private readonly Dictionary<string, Type> _codeTypes = new();

        /// <summary>
        /// Finds code by type
        /// </summary>
        public string GetCode(Type type) => _typeCodes[type];

        /// <summary>
        /// Finds type by code
        /// </summary>
        public Type GetType(string code) => _codeTypes[code];

        /// <summary>
        /// Registers a type into provider
        /// </summary>
        public void Register(Type type)
        {
            ModelTypeAttribute attribute = type.GetCustomAttribute<ModelTypeAttribute>(false);
            string code = attribute != null ? attribute.TypeCode : type.Name;

            _typeCodes.Add(type, code);
            _codeTypes.Add(code, type);
        }

        /// <summary>
        /// Reads websocket message and deserializes model
        /// </summary>
        public object Get(WebSocketMessage message, Type modelType)
        {
            message.Content.Position = 0;

            Type openGeneric = typeof(PayloadFrame<>);
            Type genericType = openGeneric.MakeGenericType(modelType);

            object model = System.Text.Json.JsonSerializer.DeserializeAsync(message.Content, genericType);
            return model;
        }

        /// <summary>
        /// Creates new websocket message and writes the model 
        /// </summary>
        public WebSocketMessage Write(object model)
        {
            Type type = model.GetType();
            string code;
            if (_typeCodes.ContainsKey(type))
                code = _typeCodes[type];
            else
            {
                ModelTypeAttribute attr = type.GetCustomAttribute<ModelTypeAttribute>();
                code = attr == null ? type.Name : attr.TypeCode;

                _typeCodes.Add(type, code);
                _codeTypes.Add(code, type);
            }

            var obj = new
                      {
                          type = code,
                          payload = model
                      };

            string content = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            return WebSocketMessage.FromString(content);
        }

        /// <summary>
        /// Resolves model type from websocket message
        /// </summary>
        public Type Resolve(WebSocketMessage message)
        {
            message.Content.Position = 0;

            PayloadResolve resolve = System.Text.Json.JsonSerializer.DeserializeAsync<PayloadResolve>(message.Content).GetAwaiter().GetResult();
            if (resolve == null || string.IsNullOrEmpty(resolve.Type))
                return null;
            Type type;
            bool found = _codeTypes.TryGetValue(resolve.Type, out type);
            if (!found)
                return null;

            return type;
        }
    }
}