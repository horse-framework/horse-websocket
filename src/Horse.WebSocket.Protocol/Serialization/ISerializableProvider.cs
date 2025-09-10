using System.Reflection;

namespace Horse.WebSocket.Protocol.Serialization;

/// <summary>
/// Serializable provider
/// </summary>
public interface ISerializableProvider : IWebSocketModelProvider
{
    /// <summary>
    /// Checks all defined models in specified assemblies and recognizes type codes.
    /// </summary>
    void WarmUp(params Assembly[] assemblies);

    /// <summary>
    /// Serializer
    /// </summary>
    public IJsonModelSerializer Serializer { get; }
}