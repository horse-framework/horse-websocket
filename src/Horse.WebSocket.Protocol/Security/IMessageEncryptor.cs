namespace Horse.WebSocket.Protocol.Security;

/// <summary>
/// Message Encryptor interface for web socket messages
/// </summary>
public interface IMessageEncryptor
{
    /// <summary>
    /// Sets encrption keys.
    /// Key1 is usually default key.
    /// Key2 is usually IV or nonce.
    /// Key3 is usually tag.
    /// </summary>
    void SetKeys(byte[] key1, byte[] key2 = null, byte[] key3 = null);

    /// <summary>
    /// Encrpts message and writes cipher data to the same message.
    /// </summary>
    void EncryptMessage(WebSocketMessage plainMessage, byte[] nonce = null);

    /// <summary>
    /// Decrypts the message and writes plain data to the same message.
    /// </summary>
    void DecryptMessage(WebSocketMessage cipherMessage, byte[] nonce = null);

    /// <summary>
    /// Encrpts the plain data and returns cipher
    /// </summary>
    byte[] EncryptData(byte[] plain, byte[] nonce = null);

    /// <summary>
    /// Decrpts the cipher data and returns plain
    /// </summary>
    byte[] DecryptData(byte[] cipher, byte[] nonce = null);
}