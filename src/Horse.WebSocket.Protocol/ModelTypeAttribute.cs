﻿using System;

namespace Horse.WebSocket.Protocol;

/// <summary>
/// Model type descriptor attribute for objects.
/// That attribute should be used on models, not handlers.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ModelTypeAttribute : Attribute
{
    /// <summary>
    /// Type code of the model
    /// </summary>
    public string TypeCode { get; }

    /// <summary>
    /// Creates new model type attribute
    /// </summary>
    public ModelTypeAttribute(string typeCode)
    {
        TypeCode = typeCode;
    }
    
    /// <summary>
    /// Creates new model type attribute
    /// </summary>
    public ModelTypeAttribute(int typeCode)
    {
        TypeCode = typeCode.ToString();
    }
}