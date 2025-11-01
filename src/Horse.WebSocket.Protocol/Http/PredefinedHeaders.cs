using System;
using System.Text;

namespace Horse.WebSocket.Protocol.Http;

/// <summary>
/// Predefined response and request header partial data for optimization
/// </summary>
public static class PredefinedHeaders
{
    internal static readonly ReadOnlyMemory<byte> HTTP_VERSION = "HTTP/1.1 "u8.ToArray();
    internal static readonly ReadOnlyMemory<byte> SERVER_CRLF = "Server: horse\r\n"u8.ToArray();
    internal static readonly ReadOnlyMemory<byte> CONTENT_LENGTH_COLON = "Content-Length: "u8.ToArray();
    internal static readonly ReadOnlyMemory<byte> CONNECTION_KEEP_ALIVE_CRLF = "Connection: keep-alive\r\n"u8.ToArray();
    internal static readonly ReadOnlyMemory<byte> CONNECTION_CLOSE_CRLF = "Connection: close\r\n"u8.ToArray();
    internal static readonly ReadOnlyMemory<byte> CONNECTION_UPGRADE_CRLF = "Connection: Upgrade\r\n"u8.ToArray();
    internal static readonly ReadOnlyMemory<byte> CHARSET_UTF8_CRLF = ";charset=UTF-8\r\n"u8.ToArray();
    internal static readonly ReadOnlyMemory<byte> CONTENT_TYPE_COLON = "Content-Type: "u8.ToArray();

    internal static readonly ReadOnlyMemory<byte> ENCODING_GZIP_CRLF = "Content-Encoding: gzip\r\n"u8.ToArray();
    internal static readonly ReadOnlyMemory<byte> ENCODING_DEFLATE_CRLF = "Content-Encoding: deflate\r\n"u8.ToArray();
    internal static readonly ReadOnlyMemory<byte> ENCODING_BR_CRLF = "Content-Encoding: br\n"u8.ToArray();

    internal static readonly byte[] CONTENT_DISPOSITION_COLON = "Content-Disposition: "u8.ToArray();
    internal static readonly byte[] CONTENT_TYPE_COLON_BYTES = "Content-Type: "u8.ToArray();
    internal static readonly byte[] CONTENT_TRANSFER_ENCODING = "Content-Transfer-Encoding: "u8.ToArray();

    internal static ReadOnlyMemory<byte> SERVER_TIME_CRLF = Encoding.ASCII.GetBytes("Date: " + DateTime.UtcNow.ToString("R") + "\r\n");

    internal static ReadOnlyMemory<byte> WEBSOCKET_101_SWITCHING_PROTOCOLS_CRLF = "HTTP/1.1 101 Switching Protocols\r\n"u8.ToArray();
    internal static ReadOnlyMemory<byte> UPGRADE_WEBSOCKET_CRLF = "Upgrade: websocket\r\n"u8.ToArray();
    internal static ReadOnlyMemory<byte> SEC_WEB_SOCKET_COLON = "Sec-WebSocket-Accept: "u8.ToArray();

    internal static readonly byte[] BOUNDARY_END = "--"u8.ToArray();

    internal const string NAME_KV_QUOTA = "name=\"";
    internal const string FILENAME_KV_QUOTA = "filename=\"";
}