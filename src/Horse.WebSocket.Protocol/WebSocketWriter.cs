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
    private readonly bool _masking;
    private static readonly Random _random = new Random();

    /// <summary>
    /// RFC-6455 Section 5.1 Rule:
    /// Client MUST use masking. Server MUST NOT use masking.
    /// You MUST set useMasking parameter value for this.
    /// </summary>
    public WebSocketWriter(bool useMasking)
    {
        _masking = useMasking;
    }

    #region Async

    /// <summary>
    /// Writes message to specified stream.
    /// </summary>
    public async Task WriteAsync(WebSocketMessage value, Stream stream, IMessageEncryptor encryptor = null)
    {
        encryptor?.EncryptMessage(value);

        byte op = (byte)value.OpCode;
        op += 0x80;

        //fin and op code
        stream.WriteByte(op);

        //length
        ulong length = 0;
        if (value.Content != null)
            length = (ulong)value.Content.Length;

        bool useEncryption = encryptor != null && length > 0 && !encryptor.SkipEncryptionTypeData;
        if (useEncryption)
            length++;

        //length
        await WriteLengthAsync(stream, length);

        if (value.Content != null)
            await WriteContentAsync(stream, value.Content.ToArray(), useEncryption, encryptor != null && !encryptor.SkipEncryptionTypeData ? encryptor.EncryptorId : null);
    }

    /// <summary>
    /// Creates byte array data of the message
    /// </summary>
    public async Task<byte[]> CreateAsync(WebSocketMessage value, IMessageEncryptor encryptor = null)
    {
        encryptor?.EncryptMessage(value);
        await using MemoryStream ms = new MemoryStream();

        byte op = (byte)value.OpCode;
        op += 0x80;

        //fin and op code
        ms.WriteByte(op);

        //length
        ulong length = 0;
        if (value.Content != null)
            length = (ulong)value.Content.Length;

        bool useEncryption = encryptor != null && length > 0 && !encryptor.SkipEncryptionTypeData;
        if (useEncryption)
            length++;

        await WriteLengthAsync(ms, length);

        if (value.Content != null)
            await WriteContentAsync(ms, value.Content.ToArray(), useEncryption, encryptor != null && !encryptor.SkipEncryptionTypeData ? encryptor.EncryptorId : null);

        return ms.ToArray();
    }

    /// <summary>
    /// Creates byte array data of the message
    /// </summary>
    public async Task<byte[]> CreateAsync(string message, IMessageEncryptor encryptor = null)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);

        if (encryptor != null)
            bytes = encryptor.EncryptData(Encoding.UTF8.GetBytes(message));

        await using MemoryStream ms = new MemoryStream();
        ms.WriteByte(0x81);

        ulong length = (ulong)bytes.Length;

        bool useEncryption = encryptor != null && length > 0 && !encryptor.SkipEncryptionTypeData;
        if (useEncryption)
            length++;

        await WriteLengthAsync(ms, length);
        await WriteContentAsync(ms, bytes, useEncryption, encryptor != null && !encryptor.SkipEncryptionTypeData ? encryptor.EncryptorId : null);
        return ms.ToArray();
    }

    /// <summary>
    /// Writes length of the message with websocket protocol
    /// </summary>
    private async Task WriteLengthAsync(Stream stream, ulong length)
    {
        //1 byte length
        if (length < 126)
            stream.WriteByte((byte)(length + (_masking ? (ulong)128 : 0)));

        //3 (1 + ushort) bytes length
        else if (length <= ushort.MaxValue)
        {
            stream.WriteByte((byte)(126 + (_masking ? 128 : 0)));
            ushort len = (ushort)length;
            byte[] lengthBytes = BitConverter.GetBytes(len);
            await stream.WriteAsync(new[] { lengthBytes[1], lengthBytes[0] }, 0, 2);
        }

        //9 (1 + ulong) bytes length
        else
        {
            stream.WriteByte((byte)(127 + (_masking ? 128 : 0)));
            ulong len = length;
            byte[] lb = BitConverter.GetBytes(len);
            await stream.WriteAsync(new[] { lb[7], lb[6], lb[5], lb[4], lb[3], lb[2], lb[1], lb[0] }, 0, 8);
        }
    }

    #endregion

    #region Sync

    /// <summary>
    /// Writes message to specified stream.
    /// </summary>
    public void Write(WebSocketMessage value, Stream stream, IMessageEncryptor encryptor = null)
    {
        encryptor?.EncryptMessage(value);

        byte op = (byte)value.OpCode;
        op += 0x80;

        //fin and op code
        stream.WriteByte(op);

        //length
        ulong length = 0;
        if (value.Content != null)
            length = (ulong)value.Content.Length;

        bool useEncryption = encryptor != null && length > 0;
        if (useEncryption)
            length++;

        //length
        WriteLength(stream, length);

        if (value.Content != null)
            WriteContent(stream, value.Content.ToArray(), useEncryption, encryptor?.EncryptorId);
    }

    /// <summary>
    /// Creates byte array data of the message
    /// </summary>
    public byte[] Create(WebSocketMessage value, IMessageEncryptor encryptor = null)
    {
        encryptor?.EncryptMessage(value);

        using MemoryStream ms = new MemoryStream();

        byte op = (byte)value.OpCode;
        op += 0x80;

        //fin and op code
        ms.WriteByte(op);

        //length
        ulong length = 0;
        if (value.Content != null)
            length = (ulong)value.Content.Length;

        bool useEncryption = encryptor != null && length > 0;
        if (useEncryption)
            length++;

        WriteLength(ms, length);

        if (value.Content != null)
            WriteContent(ms, value.Content.ToArray(), useEncryption, encryptor?.EncryptorId);

        return ms.ToArray();
    }

    /// <summary>
    /// Creates byte array data of the message
    /// </summary>
    public byte[] Create(string message, IMessageEncryptor encryptor = null)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);

        if (encryptor != null)
            bytes = encryptor.EncryptData(Encoding.UTF8.GetBytes(message));

        ulong length = (ulong)bytes.Length;
        using MemoryStream ms = new MemoryStream();
        ms.WriteByte(0x81);

        bool useEncryption = encryptor != null && length > 0;
        if (useEncryption)
            length++;

        WriteLength(ms, length);
        WriteContent(ms, bytes, useEncryption, encryptor?.EncryptorId);
        return ms.ToArray();
    }

    /// <summary>
    /// Writes length of the message with websocket protocol
    /// </summary>
    private void WriteLength(Stream stream, ulong length)
    {
        //1 byte length
        if (length < 126)
            stream.WriteByte((byte)(length + (_masking ? (ulong)128 : 0)));

        //3 (1 + ushort) bytes length
        else if (length <= ushort.MaxValue)
        {
            stream.WriteByte((byte)(126 + (_masking ? 128 : 0)));
            ushort len = (ushort)length;
            byte[] lenbytes = BitConverter.GetBytes(len);
            stream.Write(new[] { lenbytes[1], lenbytes[0] }, 0, 2);
        }

        //9 (1 + ulong) bytes length
        else
        {
            stream.WriteByte((byte)(127 + (_masking ? 128 : 0)));
            byte[] lb = BitConverter.GetBytes(length);
            stream.Write(new[] { lb[7], lb[6], lb[5], lb[4], lb[3], lb[2], lb[1], lb[0] }, 0, 8);
        }
    }

    #endregion

    private void WriteContent(Stream destination, byte[] data, bool useEncryption, byte? encryptorKey)
    {
        if (_masking)
        {
            byte[] mask = BitConverter.GetBytes(_random.Next(int.MaxValue / 2, int.MaxValue));
            int maskIndexPadding = 0;

            destination.Write(mask);

            if (useEncryption && encryptorKey.HasValue)
            {
                destination.WriteByte((byte)(encryptorKey.Value ^ mask[maskIndexPadding]));
                maskIndexPadding = 1;
            }

            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)(data[i] ^ mask[(i + maskIndexPadding) % 4]);

            destination.Write(data);
        }
        else
        {
            if (useEncryption && encryptorKey.HasValue)
                destination.WriteByte(encryptorKey.Value);

            destination.Write(data);
        }
    }

    private async Task WriteContentAsync(Stream destination, byte[] data, bool useEncryption, byte? encryptorKey)
    {
        if (_masking)
        {
            byte[] mask = BitConverter.GetBytes(_random.Next(int.MaxValue / 2, int.MaxValue));
            int maskIndexPadding = 0;

            destination.Write(mask);

            if (useEncryption && encryptorKey.HasValue)
            {
                destination.WriteByte((byte)(encryptorKey.Value ^ mask[maskIndexPadding]));
                maskIndexPadding = 1;
            }

            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)(data[i] ^ mask[(i + maskIndexPadding) % 4]);

            await destination.WriteAsync(data);
        }
        else
        {
            if (useEncryption && encryptorKey.HasValue)
                destination.WriteByte(encryptorKey.Value);

            await destination.WriteAsync(data);
        }
    }
}