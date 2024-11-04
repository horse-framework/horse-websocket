using System;
using System.Collections.Generic;
using Horse.WebSocket.Protocol.Security;

namespace Horse.WebSocket.Protocol;

public class EncryptorContainer
{
    private readonly Dictionary<byte, IMessageEncryptor> _encryptors = new Dictionary<byte, IMessageEncryptor>();

    public bool HasAnyEncryptor { get; private set; }
    public byte DefaultKey { get; set; } = 1;

    internal EncryptorContainer Clone()
    {
        EncryptorContainer clone = new EncryptorContainer();
        foreach (KeyValuePair<byte,IMessageEncryptor> pair in _encryptors)
            clone._encryptors.Add(pair.Key, pair.Value);

        clone.DefaultKey = DefaultKey;
        clone.HasAnyEncryptor = HasAnyEncryptor;
        return clone;
    }
    
    public void SetEncryptor(IMessageEncryptor encryptor)
    {
        if (!HasAnyEncryptor)
            DefaultKey = encryptor.Key;
        
        HasAnyEncryptor = true;
        _encryptors[encryptor.Key] = encryptor;
    }

    public void SetDefaultEncryptor(IMessageEncryptor encryptor)
    {
        if (encryptor == null)
            return;

        HasAnyEncryptor = true;
        DefaultKey = encryptor.Key;
        _encryptors[encryptor.Key] = encryptor;
    }

    public IMessageEncryptor GetEncryptor(byte number)
    {
        if (!HasAnyEncryptor)
            return null;

        if (number == 0)
            throw new InvalidOperationException("Zero Number Key must be plain text");

        _encryptors.TryGetValue(number, out var encryptor);
        return encryptor;
    }

    public IMessageEncryptor GetDefaultEncryptor()
    {
        if (!HasAnyEncryptor)
            return null;

        return GetEncryptor(DefaultKey);
    }
}