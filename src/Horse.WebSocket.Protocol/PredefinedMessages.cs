using System.Text;

namespace Horse.WebSocket.Protocol;

/// <summary>
/// Predefined messages for Websocket protocol
/// </summary>
public static class PredefinedMessages
{
    public const byte CR = (byte)'\r';
    public const byte LF = (byte)'\n';
    public const byte COLON = (byte)':';
    public const byte SEMICOLON = (byte)';';
    public const byte QMARK = (byte)'?';
    public const byte AND = (byte)'&';
    public const byte EQUALS = (byte)'=';
    public const byte SPACE = (byte)' ';
    public static readonly byte[] CRLF = { CR, LF };
    public static readonly byte[] COLON_SPACE = { COLON, SPACE };

    /// <summary>
    /// Websocket PING message 0x89 0x00
    /// </summary>
    public static readonly byte[] PING = { 0x89, 0x00 };

    /// <summary>
    /// Websocket PONG message 0x8A 0x00
    /// </summary>
    public static readonly byte[] PONG = { 0x8A, 0x00 };

    /// <summary>
    /// HTTP Header, "Sec-WebSocket-Key"
    /// </summary>
    public const string WEBSOCKET_KEY = "Sec-WebSocket-Key";

    /// <summary>
    /// "HTTP/1.1 " as bytes
    /// </summary>
    public static readonly byte[] HTTP_VERSION = Encoding.ASCII.GetBytes("HTTP/1.1 ");

    /// <summary>
    /// "Server: horse\r\n" as bytes
    /// </summary>
    public static readonly byte[] SERVER_CRLF = Encoding.ASCII.GetBytes("Server: horse\r\n");

    /// <summary>
    /// "Connection: Upgrade\r\n" as bytes
    /// </summary>
    public static readonly byte[] CONNECTION_UPGRADE_CRLF = Encoding.ASCII.GetBytes("Connection: Upgrade\r\n");

    /// <summary>
    /// "HTTP/1.1 101 Switching Protocols\r\n" as bytes
    /// </summary>
    public static readonly byte[] WEBSOCKET_101_SWITCHING_PROTOCOLS_CRLF = Encoding.ASCII.GetBytes("HTTP/1.1 101 Switching Protocols\r\n");

    /// <summary>
    /// "Upgrade: websocket\r\n" as bytes
    /// </summary>
    public static readonly byte[] UPGRADE_WEBSOCKET_CRLF = Encoding.ASCII.GetBytes("Upgrade: websocket\r\n");

    /// <summary>
    /// "Sec-WebSocket-Accept: " as bytes
    /// </summary>
    public static readonly byte[] SEC_WEB_SOCKET_COLON = Encoding.ASCII.GetBytes("Sec-WebSocket-Accept: ");

    /// <summary>
    /// "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
    /// </summary>
    public const string WEBSOCKET_GUID = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
}