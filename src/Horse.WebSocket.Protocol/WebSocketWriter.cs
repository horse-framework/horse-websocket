using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Horse.WebSocket.Protocol.Security;

namespace Horse.WebSocket.Protocol;

/// <summary>
/// WebSocket Protocol message writer
/// </summary>
public class WebSocketWriter
{
    #region Async

    /// <summary>
    /// Writes message to specified stream.
    /// </summary>
    public async Task WriteAsync(WebSocketMessage value, Stream stream, IMessageEncryptor encryptor)
    {
        encryptor?.EncryptMessage(value);

        byte op = (byte) value.OpCode;
        op += 0x80;

        //fin and op code
        stream.WriteByte(op);

        //length
        ulong length = 0;
        if (value.Content != null)
            length = (ulong) value.Content.Length;

        //length
        await WriteLengthAsync(stream, length);

        if (value.Content != null)
            value.Content.WriteTo(stream);
    }

    /// <summary>
    /// Creates byte array data of the message
    /// </summary>
    public async Task<byte[]> CreateAsync(WebSocketMessage value, IMessageEncryptor encryptor)
    {
        encryptor?.EncryptMessage(value);
        await using MemoryStream ms = new MemoryStream();

        byte op = (byte) value.OpCode;
        op += 0x80;

        //fin and op code
        ms.WriteByte(op);

        //length
        ulong length = 0;
        if (value.Content != null)
            length = (ulong) value.Content.Length;

        await WriteLengthAsync(ms, length);

        if (value.Content != null)
            value.Content.WriteTo(ms);

        return ms.ToArray();
    }

    /// <summary>
    /// Creates byte array data of the message
    /// </summary>
    public async Task<byte[]> CreateAsync(string message, IMessageEncryptor encryptor)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);

        if (encryptor != null)
            bytes = encryptor.EncryptData(Encoding.UTF8.GetBytes(message));

        await using MemoryStream ms = new MemoryStream();
        ms.WriteByte(0x81);
        await WriteLengthAsync(ms, (ulong) bytes.Length);
        await ms.WriteAsync(bytes);
        return ms.ToArray();
    }

    /// <summary>
    /// Creates byte array data of only message header frame
    /// </summary>
    public async Task<byte[]> CreateFrameAsync(WebSocketMessage value)
    {
        await using MemoryStream ms = new MemoryStream();

        byte op = (byte) value.OpCode;
        op += 0x80;

        //fin and op code
        ms.WriteByte(op);

        //length
        ulong length = 0;
        if (value.Content != null)
            length = (ulong) value.Content.Length;

        //length
        await WriteLengthAsync(ms, length);

        return ms.ToArray();
    }

    /// <summary>
    /// Creates byte array data of only message content
    /// </summary>
    public async Task<byte[]> CreateContentAsync(WebSocketMessage value)
    {
        await using MemoryStream ms = new MemoryStream();
        value.Content.WriteTo(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// Writes length of the message with websocket protocol
    /// </summary>
    private static async Task WriteLengthAsync(Stream stream, ulong length)
    {
        //1 byte length
        if (length < 126)
            stream.WriteByte((byte) length);

        //3 (1 + ushort) bytes length
        else if (length <= ushort.MaxValue)
        {
            stream.WriteByte(126);
            ushort len = (ushort) length;
            Memory<byte> lenbytes = BitConverter.GetBytes(len);
            await stream.WriteAsync(lenbytes);
        }

        //9 (1 + ulong) bytes length
        else
        {
            stream.WriteByte(127);
            ulong len = length;
            Memory<byte> lb = BitConverter.GetBytes(len);
            await stream.WriteAsync(lb);
        }
    }

    #endregion

    #region Sync

    /// <summary>
    /// Writes message to specified stream.
    /// </summary>
    public void Write(WebSocketMessage value, Stream stream, IMessageEncryptor encryptor)
    {
        encryptor?.EncryptMessage(value);

        byte op = (byte) value.OpCode;
        op += 0x80;

        //fin and op code
        stream.WriteByte(op);

        //length
        ulong length = 0;
        if (value.Content != null)
            length = (ulong) value.Content.Length;

        //length
        WriteLength(stream, length);

        if (value.Content != null)
            value.Content.WriteTo(stream);
    }

    /// <summary>
    /// Creates byte array data of the message
    /// </summary>
    public byte[] Create(WebSocketMessage value, IMessageEncryptor encryptor)
    {
        encryptor?.EncryptMessage(value);

        using MemoryStream ms = new MemoryStream();

        byte op = (byte) value.OpCode;
        op += 0x80;

        //fin and op code
        ms.WriteByte(op);

        //length
        ulong length = 0;
        if (value.Content != null)
            length = (ulong) value.Content.Length;

        WriteLength(ms, length);

        if (value.Content != null)
            value.Content.WriteTo(ms);

        return ms.ToArray();
    }

    /// <summary>
    /// Creates byte array data of the message
    /// </summary>
    public byte[] Create(string message, IMessageEncryptor encryptor)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);

        if (encryptor != null)
            bytes = encryptor.EncryptData(Encoding.UTF8.GetBytes(message));

        using MemoryStream ms = new MemoryStream();
        ms.WriteByte(0x81);
        WriteLength(ms, (ulong) bytes.Length);
        ms.Write(bytes);
        return ms.ToArray();
    }

    /// <summary>
    /// Creates byte array data of only message header frame
    /// </summary>
    public byte[] CreateFrame(WebSocketMessage value)
    {
        using MemoryStream ms = new MemoryStream();

        byte op = (byte) value.OpCode;
        op += 0x80;

        //fin and op code
        ms.WriteByte(op);

        //length
        ulong length = 0;
        if (value.Content != null)
            length = (ulong) value.Content.Length;

        //length
        WriteLength(ms, length);

        return ms.ToArray();
    }

    /// <summary>
    /// Creates byte array data of only message content
    /// </summary>
    public byte[] CreateContent(WebSocketMessage value)
    {
        using MemoryStream ms = new MemoryStream();
        value.Content.WriteTo(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// Writes length of the message with websocket protocol
    /// </summary>
    private static void WriteLength(Stream stream, ulong length)
    {
        //1 byte length
        if (length < 126)
            stream.WriteByte((byte) length);

        //3 (1 + ushort) bytes length
        else if (length <= ushort.MaxValue)
        {
            stream.WriteByte(126);
            ushort len = (ushort) length;
            ReadOnlySpan<byte> lenbytes = BitConverter.GetBytes(len);
            stream.Write(lenbytes);
        }

        //9 (1 + ulong) bytes length
        else
        {
            stream.WriteByte(127);
            ulong len = length;
            ReadOnlySpan<byte> lb = BitConverter.GetBytes(len);
            stream.Write(lb);
        }
    }

    #endregion
}