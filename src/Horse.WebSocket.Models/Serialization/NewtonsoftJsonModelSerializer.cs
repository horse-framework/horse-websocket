using System;
using Newtonsoft.Json;

namespace Horse.WebSocket.Models.Serialization
{
    /// <summary>
    /// JSON Model serializer uses Newtonsoft JSON library
    /// </summary>
    public class NewtonsoftJsonModelSerializer : IJsonModelSerializer
    {
        /// <summary>
        /// Newtonsoft JSON serialization settings
        /// </summary>
        public JsonSerializerSettings Settings { get; set; }

        /// <summary>
        /// Serializes model and returns string
        /// </summary>
        public string Serialize(object model)
        {
            return JsonConvert.SerializeObject(model, Settings);
        }

        /// <summary>
        /// Deserializes model from string
        /// </summary>
        public object Deserialize(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type, Settings);
        }
    }
}