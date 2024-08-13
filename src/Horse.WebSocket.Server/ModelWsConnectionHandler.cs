using System;
using System.Threading.Tasks;
using Horse.Core;
using Horse.Core.Protocols;
using Horse.WebSocket.Protocol;
using Horse.WebSocket.Protocol.Providers;
using Horse.WebSocket.Protocol.Serialization;

namespace Horse.WebSocket.Server;

/// <summary>
/// Executed when a client is connected to the server.
/// The client class should be decided, instantiated in this method.
/// </summary>
public delegate Task<WsServerSocket> ConnectedHandler(IServiceProvider services, IConnectionInfo info, ConnectionData data);

/// <summary>
/// Executed after client is connected and completed all handshaking operations.
/// </summary>
public delegate Task ClientReadyHandler(IServiceProvider services, WsServerSocket client);

/// <summary>
/// Executed when a client sends a message to the server
/// </summary>
public delegate Task MessageReceivedHandler(IServiceProvider services, WebSocketMessage message, WsServerSocket socket);

/// <summary>
/// Executed when a client is disconnected from the server
/// </summary>
public delegate Task DisconnectedHandler(IServiceProvider services, WsServerSocket socket);

/// <summary>
/// Model based websocket connection handler
/// </summary>
internal sealed class ModelWsConnectionHandler : IWebSocketServerBus, IProtocolConnectionHandler<WsServerSocket, WebSocketMessage>
{
    #region Properties

    /// <summary>
    /// Message observer for websocket connection handler
    /// </summary>
    public WebSocketMessageObserver Observer { get; internal set; }

    internal ConnectedHandler ConnectedFunc { get; set; }
    internal ClientReadyHandler ReadyAction { get; set; }
    internal MessageReceivedHandler MessageReceivedAction { get; set; }
    internal DisconnectedHandler DisconnectedAction { get; set; }

    private WebSocketErrorHandler _errorAction;

    internal WebSocketErrorHandler ErrorAction
    {
        get => _errorAction;
        set
        {
            _errorAction = value;
            if (Observer != null)
                Observer.ErrorAction = value;
        }
    }

    internal IServiceProvider ServiceProvider { get; set; }

    #endregion

    internal ModelWsConnectionHandler()
    {
        Observer = new WebSocketMessageObserver(new PipeModelProvider(new NewtonsoftJsonModelSerializer()), ErrorAction);
    }

    #region Events

    /// <summary>
    /// Connected event
    /// </summary>
    public async Task<WsServerSocket> Connected(IHorseServer server, IConnectionInfo connection, ConnectionData data)
    {
        WsServerSocket socket;

        if (ConnectedFunc != null)
            socket = await ConnectedFunc(ServiceProvider, connection, data);
        else
            socket = new WsServerSocket(server, connection);

        return socket;
    }

    /// <summary>
    /// Client ready event
    /// </summary>
    public Task Ready(IHorseServer server, WsServerSocket client)
    {
        if (ReadyAction != null)
            return ReadyAction(ServiceProvider, client);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Message received event
    /// </summary>
    public async Task Received(IHorseServer server, IConnectionInfo info, WsServerSocket client, WebSocketMessage message)
    {
        await Observer.Read(message, client);

        if (MessageReceivedAction != null)
            await MessageReceivedAction(ServiceProvider, message, client);
    }

    /// <summary>
    /// Client disconnected event
    /// </summary>
    public Task Disconnected(IHorseServer server, WsServerSocket client)
    {
        if (DisconnectedAction != null)
            return DisconnectedAction(ServiceProvider, client);

        return Task.CompletedTask;
    }

    #endregion

    #region Actions

    /// <summary>
    /// Sends a model to a receiver client
    /// </summary>
    public Task<bool> SendAsync<TModel>(IHorseWebSocket target, TModel model)
    {
        WebSocketMessage message = Observer.Provider.Write(model);
        return target.SendAsync(message);
    }

    public Task<bool> SendAsync<TModel>(IHorseWebSocket target, TModel model, byte encryptorNumber)
    {
        WebSocketMessage message = Observer.Provider.Write(model);
        return target.SendAsync(message, encryptorNumber);
    }

    /// <summary>
    /// Removes client from server
    /// </summary>
    public void Disconnect(IHorseWebSocket client)
    {
        client.Disconnect();
    }

    /// <inheritdoc />
    public IWebSocketModelProvider GetModelProvider() => Observer.Provider;

    #endregion
}