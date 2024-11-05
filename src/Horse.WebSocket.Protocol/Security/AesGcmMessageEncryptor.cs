using System;
using System.IO;
using System.Security.Cryptography;

namespace Horse.WebSocket.Protocol.Security;

/// <inheritdoc/>
public class AesGcmMessageEncryptor : IMessageEncryptor
{
    private AesGcm _gcm;

    private byte[] _key;
    private byte[] _defaultNonce;
    private byte[] _tag;

    public byte EncryptorId { get; set; } = 1;

    /// <inheritdoc/>
    public void SetKeys(byte[] key1, byte[] key2 = null, byte[] key3 = null)
    {
        if (key1.Length != 32)
            throw new InvalidOperationException("AES GCM key must be 256 bits");

        if (key2 != null && key2.Length != 12)
            throw new InvalidOperationException("AES GCM nonce must be 96 bits");

        if (key3.Length != 16)
            throw new InvalidOperationException("AES GCM Tag length must be 128 bits");

        _key = key1;
        _defaultNonce = key2;
        _tag = key3;

        _gcm = new AesGcm(key1);
    }

    /// <inheritdoc/>
    public void EncryptMessage(WebSocketMessage plainMessage, byte[] nonce = null)
    {
        byte[] plain = plainMessage.Content.ToArray();
        byte[] cipher = new byte[plain.Length];
        _gcm.Encrypt(nonce ?? _defaultNonce, plain, cipher, _tag);
        plainMessage.Content = new MemoryStream(cipher);
    }

    /// <inheritdoc/>
    public void DecryptMessage(WebSocketMessage cipherMessage, byte[] nonce = null)
    {
        byte[] cipher = cipherMessage.Content.ToArray();
        byte[] plaintext = new byte[cipher.Length];
        _gcm.Decrypt(nonce ?? _defaultNonce, cipher, _tag, plaintext);
        cipherMessage.Content = new MemoryStream(plaintext);
    }

    /// <inheritdoc/>
    public byte[] EncryptData(byte[] plain, byte[] nonce = null)
    {
        byte[] cipher = new byte[plain.Length];
        _gcm.Encrypt(nonce ?? _defaultNonce, plain, cipher, _tag);
        return cipher;
    }

    /// <inheritdoc/>
    public byte[] DecryptData(byte[] cipher, byte[] nonce = null)
    {
        byte[] plaintext = new byte[cipher.Length];
        _gcm.Decrypt(nonce ?? _defaultNonce, cipher, _tag, plaintext);
        return plaintext;
    }

    /// <inheritdoc/>
    public IMessageEncryptor Clone()
    {
        AesGcmMessageEncryptor clone = new AesGcmMessageEncryptor();
        clone.SetKeys(_key, _defaultNonce, _tag);
        return clone;
    }
}