using System;

namespace Horse.WebSocket.Protocol;

/// <summary>
/// Message type descriptor attribute for objects.
/// That attribute should be used on models, not handlers.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class BinaryMessageTypeAttribute : Attribute
{
    /// <summary>
    /// Type code of the model
    /// </summary>
    public short TypeCode { get; }

    /// <summary>
    /// Creates new model type attribute
    /// </summary>
    public BinaryMessageTypeAttribute(short typeCode)
    {
        TypeCode = typeCode;
    }
}