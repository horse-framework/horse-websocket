using System.Threading.Tasks;

namespace Horse.WebSocket.Protocol;

/// <summary>
/// WebSocket client implementation
/// </summary>
public interface IHorseWebSocket
{
    /// <summary>
    /// Sends a websocket message
    /// </summary>
    Task<bool> SendAsync(WebSocketMessage message);

    /// <summary>
    /// Sends a websocket message
    /// </summary>
    Task<bool> SendAsync(WebSocketMessage message, byte encryptorNumber);
    
    /// <summary>
    /// Closes client connection
    /// </summary>
    void Disconnect();

    /// <summary>
    /// Gets Tag of the client
    /// </summary>
    string GetTag();
}