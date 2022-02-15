using System;
using System.Text.Json;

namespace Horse.WebSocket.Protocol.Serialization;

/// <summary>
/// JSON Model serializer uses System.Text.Json
/// </summary>
public class SystemJsonModelSerializer : IJsonModelSerializer
{
    /// <summary>
    /// System.Text.Json serialization settings
    /// </summary>
    public JsonSerializerOptions Options { get; set; }

    /// <summary>
    /// Serializes model and returns string
    /// </summary>
    public string Serialize(object model)
    {
        return JsonSerializer.Serialize(model, model.GetType(), Options);
    }

    /// <summary>
    /// Deserializes model from string
    /// </summary>
    public object Deserialize(string json, Type type)
    {
        return JsonSerializer.Deserialize(json, type, Options);
    }
}