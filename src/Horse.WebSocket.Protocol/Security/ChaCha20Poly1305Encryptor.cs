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

        if (key2.Length != 12)
            throw new InvalidOperationException("ChaCha20-Poly1305 Nonce length must be 96 bits");

        if (key3.Length != 16)
            throw new InvalidOperationException("ChaCha20-Poly1305 Tag length must be 128 bits");

        _defaultNonce = key2;
        _tag = key3;
    }

    /// <inheritdoc/>
    public void EncryptMessage(WebSocketMessage plainMessage)
    {
        byte[] plain = plainMessage.Content.ToArray();
        ReadOnlySpan<byte> nonce = _defaultNonce;
        byte[] cipher = new byte[plain.Length];
        _cc20.Encrypt(nonce, plain, cipher, _tag);
        plainMessage.Content = new MemoryStream(cipher);
    }

    /// <inheritdoc/>
    public void DecryptMessage(WebSocketMessage cipherMessage)
    {
        byte[] cipher = cipherMessage.Content.ToArray();
        ReadOnlySpan<byte> nonce = _defaultNonce;
        byte[] plaintext = new byte[cipher.Length];
        _cc20.Decrypt(nonce, cipher, _tag, plaintext);
        cipherMessage.Content = new MemoryStream(plaintext);
    }

    /// <inheritdoc/>
    public byte[] EncryptData(byte[] plain)
    {
        byte[] cipher = new byte[plain.Length];
        _cc20.Encrypt(_defaultNonce, plain, cipher, _tag);
        return cipher;
    }

    /// <inheritdoc/>
    public byte[] DecryptData(byte[] cipher)
    {
        byte[] plaintext = new byte[cipher.Length];
        _cc20.Decrypt(_defaultNonce, cipher, _tag, plaintext);
        return plaintext;
    }
}