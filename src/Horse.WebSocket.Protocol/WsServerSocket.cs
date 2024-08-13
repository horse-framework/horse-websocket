using System;
using System.IO;
using System.Threading.Tasks;
using Horse.Core;

namespace Horse.WebSocket.Protocol;

/// <summary>
/// Websocket Server socket object
/// </summary>
public class WsServerSocket : SocketBase, IHorseWebSocket
{
    /// <summary>
    /// WebSocketWriter singleton instance
    /// </summary>
    private static readonly WebSocketWriter _writer = new WebSocketWriter(false);

    /// <summary>
    /// Server of the socket
    /// </summary>
    public IHorseServer Server { get; }

    /// <summary>
    /// Socket's connection information
    /// </summary>
    public IConnectionInfo Info { get; }

    /// <summary>
    /// True, if user is authenticated
    /// </summary>
    public bool IsAuthenticated { get; set; }

    /// <summary>
    /// Encryptor Container
    /// </summary>
    public EncryptorContainer EncryptorContainer { get; internal set; } = new EncryptorContainer();

    private Action<WsServerSocket> _cleanupAction;

    /// <summary>
    /// Creates new server-side websocket client
    /// </summary>
    public WsServerSocket(IHorseServer server, IConnectionInfo info)
        : base(info)
    {
        Client = info.Client;
        Server = server;
        Info = info;
    }

    /// <summary>
    /// Completed disconnected operations for websocket client
    /// </summary>
    protected override void OnDisconnected()
    {
        if (_cleanupAction != null)
            _cleanupAction(this);

        base.OnDisconnected();
    }

    /// <summary>
    /// Runs cleanup action
    /// </summary>
    internal void SetCleanupAction(Action<WsServerSocket> action)
    {
        _cleanupAction = action;
    }

    /// <summary>
    /// Updates keys of message encryptor
    /// </summary>
    /// <param name="number">Encryptor number</param>
    /// <param name="key1">Usually primary key</param>
    /// <param name="key2">Usually nonce</param>
    /// <param name="key3">Usually tag</param>
    public void UpdateEncryptorKeys(byte number, byte[] key1, byte[] key2, byte[] key3)
    {
        var encryptor = EncryptorContainer.GetEncryptor(number);

        if (encryptor == null)
            throw new NullReferenceException("Client does not have specified encryptor");

        encryptor.SetKeys(key1, key2, key3);
    }

    /// <summary>
    /// Sends websocket ping message
    /// </summary>
    public override void Ping()
    {
        Send(PredefinedMessages.PING);
    }

    /// <summary>
    /// Sends websocket pong message
    /// </summary>
    public override void Pong(object pingMessage = null)
    {
        if (pingMessage == null)
        {
            Send(PredefinedMessages.PONG);
            return;
        }

        WebSocketMessage ping = pingMessage as WebSocketMessage;
        if (ping == null)
        {
            Send(PredefinedMessages.PONG);
            return;
        }

        WebSocketMessage pong = new WebSocketMessage();
        pong.OpCode = SocketOpCode.Pong;
        pong.Masking = ping.Masking;
        if (ping.Length > 0)
            pong.Content = new MemoryStream(ping.Content.ToArray());

        byte[] data = _writer.Create(pong, null);
        Send(data);
    }

    /// <summary>
    /// Sends websocket message to client
    /// </summary>
    public bool Send(WebSocketMessage message)
    {
        byte[] data = _writer.Create(message, EncryptorContainer.GetDefaultEncryptor());
        return Send(data);
    }

    /// <summary>
    /// Sends websocket message to client
    /// </summary>
    public bool Send(byte encryptorNumber, WebSocketMessage message)
    {
        byte[] data = _writer.Create(message, EncryptorContainer.GetEncryptor(encryptorNumber));
        return Send(data);
    }

    /// <summary>
    /// Sends websocket message to client
    /// </summary>
    public async Task<bool> SendAsync(WebSocketMessage message)
    {
        byte[] data = await _writer.CreateAsync(message, EncryptorContainer.GetDefaultEncryptor());
        return Send(data);
    }

    /// <summary>
    /// Sends websocket message to client
    /// </summary>
    public async Task<bool> SendAsync(WebSocketMessage message, byte encryptorNumber)
    {
        byte[] data = await _writer.CreateAsync(message, EncryptorContainer.GetEncryptor(encryptorNumber));
        return Send(data);
    }

    /// <summary>
    /// Sends string message to client
    /// </summary>
    public bool Send(string message)
    {
        byte[] data = _writer.Create(WebSocketMessage.FromString(message), EncryptorContainer.GetDefaultEncryptor());
        return Send(data);
    }

    /// <summary>
    /// Sends string message to client
    /// </summary>
    public bool Send(string message, byte encryptorNumber)
    {
        byte[] data = _writer.Create(WebSocketMessage.FromString(message), EncryptorContainer.GetEncryptor(encryptorNumber));
        return Send(data);
    }

    /// <summary>
    /// Sends string message to client
    /// </summary>
    public async Task SendAsync(string message)
    {
        byte[] data = await _writer.CreateAsync(WebSocketMessage.FromString(message), EncryptorContainer.GetDefaultEncryptor());
        Send(data);
    }

    /// <summary>
    /// Sends string message to client
    /// </summary>
    public async Task SendAsync(string message, byte encryptorNumber)
    {
        byte[] data = await _writer.CreateAsync(WebSocketMessage.FromString(message), EncryptorContainer.GetDefaultEncryptor());
        Send(data);
    }
}