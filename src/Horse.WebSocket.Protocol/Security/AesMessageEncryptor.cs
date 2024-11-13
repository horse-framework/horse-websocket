using System;
using System.IO;
using System.Security.Cryptography;

namespace Horse.WebSocket.Protocol.Security;

/// <inheritdoc/>
public class AesMessageEncryptor : IMessageEncryptor
{
    private byte[] _key;
    private byte[] _iv;
    private Aes _aes = Aes.Create();

    /// <inheritdoc/>
    public byte EncryptorId { get; set; } = 2;

    /// <inheritdoc/>
    public bool CloneForEachConnection { get; set; }

    /// <summary>
    /// Key1 is AES RGB Key. Key2 is AES IV (Initialization vector). Key3 in unused.
    /// </summary>
    /// <param name="key1">AES RGB Key</param>
    /// <param name="key2">AES IV (Initialization vector)</param>
    /// <param name="key3">Unused</param>
    public void SetKeys(byte[] key1, byte[] key2 = null, byte[] key3 = null)
    {
        _key = key1;
        _iv = key2;

        if (key2.Length != 16)
            throw new InvalidOperationException("AES IV Vector size must be 128 bits");

        _aes = Aes.Create();
        _aes.Key = _key;
        _aes.IV = _iv;
    }

    /// <inheritdoc/>
    public void EncryptMessage(WebSocketMessage plainMessage, byte[] nonce = null)
    {
        byte[] plain = plainMessage.Content.ToArray();
        plainMessage.Content = new MemoryStream(EncryptData(plain));
    }

    /// <inheritdoc/>
    public void DecryptMessage(WebSocketMessage cipherMessage, byte[] nonce = null)
    {
        byte[] plain = cipherMessage.Content.ToArray();
        cipherMessage.Content = new MemoryStream(DecryptData(plain));
    }

    /// <inheritdoc/>
    public byte[] EncryptData(byte[] plain, byte[] nonce = null)
    {
        using MemoryStream ms = new MemoryStream();

        using (CryptoStream cs = new CryptoStream(ms, _aes.CreateEncryptor(), CryptoStreamMode.Write))
            cs.Write(plain, 0, plain.Length);

        return ms.ToArray();
    }

    /// <inheritdoc/>
    public byte[] DecryptData(byte[] cipher, byte[] nonce = null)
    {
        using MemoryStream ms = new MemoryStream();

        using (CryptoStream cs = new CryptoStream(ms, _aes.CreateDecryptor(), CryptoStreamMode.Write))
            cs.Write(cipher);

        return ms.ToArray();
    }

    /// <inheritdoc/>
    public IMessageEncryptor Clone()
    {
        AesMessageEncryptor clone = new AesMessageEncryptor();
        clone.SetKeys(_key, _iv);
        return clone;
    }
}