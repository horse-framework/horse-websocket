namespace Horse.WebSocket.Protocol.Security;

public interface IMessageEncryptor
{
    void SetKeys(byte[] key1, byte[] key2 = null, byte[] key3 = null);

    void EncryptMessage(WebSocketMessage plainMessage);

    void DecryptMessage(WebSocketMessage cipherMessage);

    byte[] EncryptData(byte[] plain);

    byte[] DecryptData(byte[] cipher);
}