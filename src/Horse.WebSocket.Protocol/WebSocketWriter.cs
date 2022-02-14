using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Horse.WebSocket.Protocol;

/// <summary>
/// WebSocket Protocol message writer
/// </summary>
public class WebSocketWriter
{
    /// <summary>
    /// Writes message to specified stream.
    /// </summary>
    public async Task Write(WebSocketMessage value, Stream stream)
    {
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
    public async Task<byte[]> Create(WebSocketMessage value)
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

        await WriteLengthAsync(ms, length);

        if (value.Content != null)
            value.Content.WriteTo(ms);

        return ms.ToArray();
    }

    /// <summary>
    /// Creates byte array data of the message
    /// </summary>
    public async Task<byte[]> Create(string message)
    {
        await using MemoryStream ms = new MemoryStream();
        ms.WriteByte(0x81);
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        await WriteLengthAsync(ms, (ulong) bytes.Length);
        await ms.WriteAsync(bytes);
        return ms.ToArray();
    }

    /// <summary>
    /// Creates byte array data of only message header frame
    /// </summary>
    public async Task<byte[]> CreateFrame(WebSocketMessage value)
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
    public async Task<byte[]> CreateContent(WebSocketMessage value)
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
        else if (length <= UInt16.MaxValue)
        {
            stream.WriteByte(126);
            ushort len = (ushort) length;
            byte[] lenbytes = BitConverter.GetBytes(len);
            await stream.WriteAsync(new[] {lenbytes[1], lenbytes[0]}, 0, 2);
        }

        //9 (1 + ulong) bytes length
        else
        {
            stream.WriteByte(127);
            ulong len = length;
            byte[] lb = BitConverter.GetBytes(len);
            await stream.WriteAsync(new[] {lb[7], lb[6], lb[5], lb[4], lb[3], lb[2], lb[1], lb[0]}, 0, 8);
        }
    }
}