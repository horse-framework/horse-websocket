using System;
using System.IO;
using System.Text;

namespace Horse.WebSocket.Protocol;

/// <summary>
/// WebSocket Message
/// </summary>
public class WebSocketMessage
{
    /// <summary>
    /// WebSocket protocol OpCode (message type)
    /// </summary>
    public SocketOpCode OpCode { get; set; }

    /// <summary>
    /// True, if message is masking with a key
    /// </summary>
    public bool Masking { get; set; }

    /// <summary>
    /// If message is masking, 4 bytes key value. Otherwise null.
    /// </summary>
    public byte[] Mask { get; set; }

    /// <summary>
    /// Message length
    /// </summary>
    public long Length => Content?.Length ?? 0;

    /// <summary>
    /// Message content stream
    /// </summary>
    public MemoryStream Content { get; set; }
    
    /// <summary>
    /// Readonly Memory Content
    /// </summary>
    public ReadOnlyMemory<byte>? ReadOnlyContent { get; set; }

    /// <summary>
    /// Creates new message from string
    /// </summary>
    public static WebSocketMessage FromString(string message)
    {
        return new WebSocketMessage
        {
            OpCode = SocketOpCode.UTF8,
            Content = new MemoryStream(Encoding.UTF8.GetBytes(message))
        };
    }

    /// <summary>
    /// Reads message content as UTF-8 string
    /// </summary>
    public override string ToString()
    {
        if (Content != null)
            return Encoding.UTF8.GetString(Content.ToArray());

        return string.Empty;
    }
}