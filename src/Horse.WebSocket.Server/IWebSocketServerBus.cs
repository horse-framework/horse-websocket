using System.Threading.Tasks;
using Horse.WebSocket.Protocol;
using Horse.WebSocket.Protocol.Serialization;

namespace Horse.WebSocket.Server;

/// <summary>
/// Message Bus for websocket servers
/// </summary>
public interface IWebSocketServerBus
{
    /// <summary>
    /// Sends a message over websocket
    /// </summary>
    ValueTask<bool> SendAsync<TModel>(IHorseWebSocket target, TModel model, bool binary);

    /// <summary>
    /// Sends a message over websocket
    /// </summary>
    ValueTask<bool> SendAsync<TModel>(IHorseWebSocket target, TModel model, bool binary, byte encryptorNumber);

    /// <summary>
    /// Sends a message over websocket
    /// </summary>
    ValueTask<bool> SendBinaryAsync<TModel>(IHorseWebSocket target, TModel model) where TModel : IBinaryWebSocketModel;

    /// <summary>
    /// Sends a message over websocket
    /// </summary>
    ValueTask<bool> SendBinaryAsync<TModel>(IHorseWebSocket target, TModel model, byte encryptorNumber) where TModel : IBinaryWebSocketModel;

    /// <summary>
    /// Sends a message over websocket
    /// </summary>
    ValueTask<bool> SendTextAsync<TModel>(IHorseWebSocket target, TModel model);

    /// <summary>
    /// Sends a message over websocket
    /// </summary>
    ValueTask<bool> SendTextAsync<TModel>(IHorseWebSocket target, TModel model, byte encryptorNumber);

    /// <summary>
    /// Removes client from server
    /// </summary>
    void Disconnect(IHorseWebSocket client);

    /// <summary>
    /// Returns currently used model provider by the buss
    /// </summary>
    IWebSocketModelProvider GetTextModelProvider();

    /// <summary>
    /// Returns currently used model provider by the buss
    /// </summary>
    IWebSocketModelProvider GetBinaryModelProvider();
}