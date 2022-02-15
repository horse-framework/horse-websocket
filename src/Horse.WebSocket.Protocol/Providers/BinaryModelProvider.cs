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
    /// <summary>
    /// JSON type model serializer is not support for binary model serialization operations
    /// </summary>
    public IJsonModelSerializer Serializer => throw new NotSupportedException();

    /// <summary>
    /// For getting codes by type
    /// </summary>
    private readonly Dictionary<Type, int> _typeCodes = new Dictionary<Type, int>();

    /// <summary>
    /// For getting type by code
    /// </summary>
    private readonly Dictionary<int, Type> _codeTypes = new Dictionary<int, Type>();

    public Type Resolve(WebSocketMessage message)
    {
        if (message.OpCode != SocketOpCode.Binary)
            return null;
            
        if (message.Content.Length < 4)
            return null;

        byte[] bytes = new byte[4];
        message.Content.Read(bytes, 0, bytes.Length);
        message.Content.Position = 0;

        int code = BitConverter.ToInt32(bytes, 0);

        Type type;
        _codeTypes.TryGetValue(code, out type);
        return type;
    }

    public void Register(Type type)
    {
        ModelTypeAttribute attribute = type.GetCustomAttribute<ModelTypeAttribute>(false);
        string code = attribute != null ? attribute.TypeCode : type.Name;

        bool parse = int.TryParse(code, out int codeNumber);
        if (!parse)
            throw new InvalidOperationException("BinaryModelProvider supports only numeric Type Codes");

        _typeCodes.Add(type, codeNumber);
        _codeTypes.Add(codeNumber, type);
    }

    public object Get(WebSocketMessage message, Type modelType)
    {
        IBinaryWebSocketModel model = (IBinaryWebSocketModel) Activator.CreateInstance(modelType);
            
        message.Content.Position = 4;
            
        using BinaryReader reader = new BinaryReader(message.Content, Encoding.UTF8, true);
        model.Deserialize(reader);
            
        return model;
    }

    public WebSocketMessage Write(object model)
    {
        IBinaryWebSocketModel binaryModel = model as IBinaryWebSocketModel;
        if (binaryModel == null)
            throw new InvalidCastException("Model must have IBinaryWebSocketModel implementation");

        Type type = model.GetType();
        int code;
        if (_typeCodes.ContainsKey(type))
            code = _typeCodes[type];
        else
        {
            ModelTypeAttribute attr = type.GetCustomAttribute<ModelTypeAttribute>();
            string codeString = attr == null ? type.Name : attr.TypeCode;

            bool parse = int.TryParse(codeString, out int codeNumber);
            if (!parse)
                throw new InvalidOperationException("BinaryModelProvider supports only numeric Type Codes");

            code = codeNumber;
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

    public WebSocketMessage Write(string customCode, object model)
    {
        bool parse = int.TryParse(customCode, out int codeNumber);
        if (!parse)
            throw new InvalidOperationException("BinaryModelProvider supports only numeric Type Codes");

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