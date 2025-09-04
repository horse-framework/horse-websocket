using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Horse.WebSocket.Protocol;
using Newtonsoft.Json;

namespace Sample.ModelServer
{
    public class CustomModelFrame<T> : CustomModelFrame
    {
        [JsonProperty("d")]
        public T Data { get; set; }
    }

    public class CustomModelFrame
    {
        [JsonProperty("t")]
        public string Type { get; set; }
    }

    public class CustomModelProvider : IWebSocketModelProvider
    {
        public bool Binary => false;
        private readonly Dictionary<string, Type> _types = new(StringComparer.InvariantCultureIgnoreCase);

        public Type Resolve(WebSocketMessage message)
        {
            /*
            try
            {*/
                string msg = message.ToString();
                CustomModelFrame frame = JsonConvert.DeserializeObject<CustomModelFrame>(msg);

                if (frame == null)
                    return null;

                Type dataType;
                _types.TryGetValue(frame.Type, out dataType);
                return dataType;
            /*
            }
            catch
            {
                return null;
            }
            */
        }

        public void Register(Type type)
        {
            string key;

            TextMessageTypeAttribute typeAttribute = type.GetCustomAttribute<TextMessageTypeAttribute>();
            key = typeAttribute != null ? typeAttribute.TypeCode : type.Name;

            _types.Add(key, type);
        }

        public object Get(WebSocketMessage message, Type modelType)
        {
            string msg = message.ToString();

            Type openGeneric = typeof(CustomModelFrame<>);
            Type genericType = openGeneric.MakeGenericType(modelType);

            dynamic model = JsonConvert.DeserializeObject(msg, genericType);
            return model.Data;
        }

        public WebSocketMessage Write(object model)
        {
            Type modelType = model.GetType();
            string typeCode = _types.FirstOrDefault(x => x.Value == modelType).Key;

            /* this is an option, but you need to change this code when you changed JsonProperty attribute values
             * return WebSocketMessage.FromString(JsonConvert.SerializeObject(new {t = typeCode, d = model})); 
             */

            //or dynamic with performance penalty
            
            Type openGeneric = typeof(CustomModelFrame<>);
            Type genericType = openGeneric.MakeGenericType(modelType);

            dynamic frame = Activator.CreateInstance(genericType);
            frame.Data = model;
            frame.Type = typeCode;

            return WebSocketMessage.FromString(JsonConvert.SerializeObject(frame));
        }

        public WebSocketMessage Write(string customCode, object model)
        {
            dynamic frame = Activator.CreateInstance(model.GetType());
            frame.Data = model;
            frame.Type = customCode;

            return WebSocketMessage.FromString(JsonConvert.SerializeObject(frame));
        }
    }
}