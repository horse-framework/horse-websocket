namespace Horse.WebSocket.Protocol.Serialization;

/// <summary>
/// Serializable provider
/// </summary>
public interface ISerializableProvider : IWebSocketModelProvider
{
    /// <summary>
    /// Serializer
    /// </summary>
    public IJsonModelSerializer Serializer { get; }
}