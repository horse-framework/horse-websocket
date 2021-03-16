using System;

namespace Horse.WebSocket.Models.Serialization
{
    /// <summary>
    /// JSON Model serializer
    /// </summary>
    public interface IJsonModelSerializer
    {
        /// <summary>
        /// Serializes a model into JSON string
        /// </summary>
        string Serialize(object model);

        /// <summary>
        /// Deserializes a model from JSON string
        /// </summary>
        object Deserialize(string json, Type type);
    }
}