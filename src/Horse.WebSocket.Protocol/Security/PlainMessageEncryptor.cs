namespace Horse.WebSocket.Protocol.Security;

/// <inheritdoc/>
public class PlainMessageEncryptor : IMessageEncryptor
{
    /// <inheritdoc/>
    public byte EncryptorId { get; set; } = 0;

    /// <inheritdoc/>
    public bool CloneForEachConnection { get; set; }

    /// <inheritdoc/>
    public void SetKeys(byte[] key1, byte[] key2 = null, byte[] key3 = null)
    {
    }

    /// <inheritdoc/>
    public void EncryptMessage(WebSocketMessage plainMessage, byte[] nonce = null)
    {
    }

    /// <inheritdoc/>
    public void DecryptMessage(WebSocketMessage cipherMessage, byte[] nonce = null)
    {
    }

    /// <inheritdoc/>
    public byte[] EncryptData(byte[] plain, byte[] nonce = null)
    {
        return plain;
    }

    /// <inheritdoc/>
    public byte[] DecryptData(byte[] cipher, byte[] nonce = null)
    {
        return cipher;
    }

    /// <inheritdoc/>
    public IMessageEncryptor Clone()
    {
        PlainMessageEncryptor encryptor = new PlainMessageEncryptor();
        encryptor.EncryptorId = EncryptorId;
        encryptor.CloneForEachConnection = CloneForEachConnection;
        return encryptor;
    }
}