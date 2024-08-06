using System;
using System.IO;
using System.Security.Cryptography;

namespace Horse.WebSocket.Protocol.Security;

/// <inheritdoc />
public class ChaCha20Poly1305Encryptor : IMessageEncryptor
{
    private byte[] _defaultNonce;
    private byte[] _tag;
    private ChaCha20Poly1305 _cc20;

    public byte Key { get; } = 3;

    /// <summary>
    /// Sets nonce value for encryptor
    /// </summary>
    public void SetNonce(byte[] nonce)
    {
        if (nonce.Length != 12)
            throw new InvalidOperationException("ChaCha20-Poly1305 Nonce length must be 96 bits");

        _defaultNonce = nonce;
    }

    /// <summary>
    /// Key1 is AES RGB Key. Key2 is AES IV (Initialization vector). Key3 in unused.
    /// </summary>
    /// <param name="key1">AES RGB Key</param>
    /// <param name="key2">AES IV (Initialization vector)</param>
    /// <param name="key3">Unused</param>
    public void SetKeys(byte[] key1, byte[] key2 = null, byte[] key3 = null)
    {
        if (_cc20 != null && key1 == null && key2 != null && key3 == null)
        {
            if (key2.Length != 12)
                throw new InvalidOperationException("ChaCha20-Poly1305 Nonce length must be 96 bits");

            _defaultNonce = key2;
            return;
        }

        if (!ChaCha20Poly1305.IsSupported)
            throw new NotSupportedException("ChaCha20-Poly1305 is not support for this platform");

        if (key1.Length != 32)
            throw new InvalidOperationException("ChaCha20-Poly1305 Key length must be 256 bits");

        _cc20 = new ChaCha20Poly1305(key1);

        if (key2 != null && key2.Length != 12)
            throw new InvalidOperationException("ChaCha20-Poly1305 Nonce length must be 96 bits");

        if (key3.Length != 16)
            throw new InvalidOperationException("ChaCha20-Poly1305 Tag length must be 128 bits");

        _defaultNonce = key2;
        _tag = key3;
    }

    /// <inheritdoc/>
    public void EncryptMessage(WebSocketMessage plainMessage, byte[] nonce = null)
    {
        byte[] plain = plainMessage.Content.ToArray();
        byte[] cipher = new byte[plain.Length];
        _cc20.Encrypt(nonce ?? _defaultNonce, plain, cipher, _tag);
        plainMessage.Content = new MemoryStream(cipher);
    }

    /// <inheritdoc/>
    public void DecryptMessage(WebSocketMessage cipherMessage, byte[] nonce = null)
    {
        byte[] cipher = cipherMessage.Content.ToArray();
        byte[] plaintext = new byte[cipher.Length];
        _cc20.Decrypt(nonce ?? _defaultNonce, cipher, _tag, plaintext);
        cipherMessage.Content = new MemoryStream(plaintext);
    }

    /// <inheritdoc/>
    public byte[] EncryptData(byte[] plain, byte[] nonce = null)
    {
        byte[] cipher = new byte[plain.Length];
        _cc20.Encrypt(nonce ?? _defaultNonce, plain, cipher, _tag);
        return cipher;
    }

    /// <inheritdoc/>
    public byte[] DecryptData(byte[] cipher, byte[] nonce = null)
    {
        byte[] plaintext = new byte[cipher.Length];
        _cc20.Decrypt(nonce ?? _defaultNonce, cipher, _tag, plaintext);
        return plaintext;
    }
}