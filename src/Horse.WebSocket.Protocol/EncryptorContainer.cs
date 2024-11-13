using System;
using System.Collections.Generic;
using Horse.WebSocket.Protocol.Security;

namespace Horse.WebSocket.Protocol;

/// <summary>
/// Container class for defined encryptors
/// </summary>
public class EncryptorContainer
{
    private readonly Dictionary<byte, IMessageEncryptor> _encryptors = new Dictionary<byte, IMessageEncryptor>();

    /// <summary>
    /// True, if there is at least one encryptor defined
    /// </summary>
    public bool HasAnyEncryptor { get; private set; }

    /// <summary>
    /// Default Encryptor Id
    /// </summary>
    public byte DefaultId { get; set; } = 1;

    internal EncryptorContainer Clone()
    {
        EncryptorContainer clone = new EncryptorContainer();
        
        foreach (KeyValuePair<byte, IMessageEncryptor> pair in _encryptors)
        {
            if (pair.Value.CloneForEachConnection)
                clone._encryptors.Add(pair.Key, pair.Value.Clone());
            else
                clone._encryptors.Add(pair.Key, pair.Value);
        }

        clone.DefaultId = DefaultId;
        clone.HasAnyEncryptor = HasAnyEncryptor;
        return clone;
    }

    /// <summary>
    /// Adds or changes encryptor
    /// </summary>
    public void SetEncryptor(IMessageEncryptor encryptor)
    {
        if (!HasAnyEncryptor)
            DefaultId = encryptor.EncryptorId;

        HasAnyEncryptor = true;
        _encryptors[encryptor.EncryptorId] = encryptor;
    }

    /// <summary>
    /// Adds or sets encryptor and defines it as default
    /// </summary>
    public void SetDefaultEncryptor(IMessageEncryptor encryptor)
    {
        if (encryptor == null)
            return;

        HasAnyEncryptor = true;
        DefaultId = encryptor.EncryptorId;
        _encryptors[encryptor.EncryptorId] = encryptor;
    }

    /// <summary>
    /// Get encryptor by unique encryptor Id
    /// </summary>
    public IMessageEncryptor GetEncryptor(byte uniqueId)
    {
        if (!HasAnyEncryptor)
            return null;

        if (uniqueId == 0)
            throw new InvalidOperationException("Zero Number Key must be plain text");

        _encryptors.TryGetValue(uniqueId, out var encryptor);
        return encryptor;
    }

    /// <summary>
    /// Gets default encryptor
    /// </summary>
    public IMessageEncryptor GetDefaultEncryptor()
    {
        if (!HasAnyEncryptor)
            return null;

        return GetEncryptor(DefaultId);
    }
}