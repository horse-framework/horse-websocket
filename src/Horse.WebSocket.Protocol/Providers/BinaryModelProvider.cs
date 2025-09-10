using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Horse.WebSocket.Protocol.Serialization;

namespace Horse.WebSocket.Protocol.Providers;

/// <summary>
/// Code provider by specified Int32 type code.
/// Serialize data will be binary. First 4 bytes of each message is message type code.
/// Other bytes are can be serialized/deserialized with BinaryWriter and BinaryReader classes.  
/// </summary>
public class BinaryModelProvider : ISerializableProvider
{
    /// <inheritdoc/>
    public bool Binary => true;

    /// <summary>
    /// JSON type model serializer is not support for binary model serialization operations
    /// </summary>
    public IJsonModelSerializer Serializer => throw new NotSupportedException();

    /// <summary>
    /// For getting codes by type
    /// </summary>
    private readonly Dictionary<Type, short> _typeCodes = new Dictionary<Type, short>();

    /// <summary>
    /// For getting type by code
    /// </summary>
    private readonly Dictionary<short, Type> _codeTypes = new Dictionary<short, Type>();

    /// <inheritdoc />
    public void WarmUp(params Assembly[] assemblies)
    {
        _typeCodes.Clear();
        _codeTypes.Clear();
        
        foreach (Assembly assembly in assemblies)
        {
            foreach (Type type in assembly.GetTypes())
            {
                BinaryMessageTypeAttribute attr = type.GetCustomAttribute<BinaryMessageTypeAttribute>();
                if (attr == null)
                    continue;

                short code = Convert.ToInt16(attr.TypeCode);
                _typeCodes.Add(type, code);
                _codeTypes.Add(code, type);
            }
        }
    }

    /// <inheritdoc />
    public Type Resolve(WebSocketMessage message)
    {
        if (message.OpCode != SocketOpCode.Binary)
            return null;

        if (message.Content.Length < 2)
            return null;

        message.Content.Position = 0;

        byte[] bytes = new byte[2];
        message.Content.ReadExactly(bytes, 0, bytes.Length);

        short code = BitConverter.ToInt16(bytes, 0);

        Type type;
        _codeTypes.TryGetValue(code, out type);
        return type;
    }

    /// <inheritdoc />
    public void Register(Type type)
    {
        BinaryMessageTypeAttribute attribute = type.GetCustomAttribute<BinaryMessageTypeAttribute>(false);

        if (attribute == null)
            throw new InvalidOperationException("Binary model must have BinaryMessageTypeAttribute attribute");

        _typeCodes.Add(type, attribute.TypeCode);
        _codeTypes.Add(attribute.TypeCode, type);
    }

    /// <inheritdoc />
    public object Get(WebSocketMessage message, Type modelType)
    {
        IBinaryWebSocketModel model = (IBinaryWebSocketModel)Activator.CreateInstance(modelType);

        message.Content.Position = 2;

        using BinaryReader reader = new BinaryReader(message.Content, Encoding.UTF8, true);
        model.Deserialize(reader);

        return model;
    }

    /// <inheritdoc />
    public WebSocketMessage Write(object model)
    {
        IBinaryWebSocketModel binaryModel = model as IBinaryWebSocketModel;
        if (binaryModel == null)
            throw new InvalidCastException("Model must have IBinaryWebSocketModel implementation");

        Type type = model.GetType();
        short code;
        if (_typeCodes.TryGetValue(type, out var typeCode))
            code = typeCode;
        else
        {
            BinaryMessageTypeAttribute attr = type.GetCustomAttribute<BinaryMessageTypeAttribute>();
            
            if (attr == null)
                throw new InvalidOperationException("Binary model must have BinaryMessageTypeAttribute attribute");

            code = attr.TypeCode;
            _typeCodes.Add(type, code);
            _codeTypes.Add(code, type);
        }

        WebSocketMessage message = new WebSocketMessage();
        message.OpCode = SocketOpCode.Binary;
        message.Content = new MemoryStream();

        using BinaryWriter writer = new BinaryWriter(message.Content, Encoding.UTF8, true);
        writer.Write(code);
        binaryModel.Serialize(writer);

        return message;
    }

    /// <inheritdoc />
    public WebSocketMessage Write(string customCode, object model)
    {
        bool parse = int.TryParse(customCode, out int codeNumber);
        if (!parse)
            throw new InvalidOperationException("BinaryModelProvider supports only Int16 Type Codes");

        IBinaryWebSocketModel binaryModel = model as IBinaryWebSocketModel;
        if (binaryModel == null)
            throw new InvalidCastException("Model must have IBinaryWebSocketModel implementation");

        WebSocketMessage message = new WebSocketMessage();
        message.OpCode = SocketOpCode.Binary;
        message.Content = new MemoryStream();

        using BinaryWriter writer = new BinaryWriter(message.Content, Encoding.UTF8, true);
        writer.Write(codeNumber);
        binaryModel.Serialize(writer);

        return message;
    }
}