using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Horse.WebSocket.Protocol.Serialization;

namespace Horse.WebSocket.Protocol.Providers;

/// <summary>
/// Code provider by class name.
/// Serialize data seems like; ModelA|{name:"foo",no:123}
/// </summary>
public class PipeModelProvider : ISerializableProvider
{
    private const char COLON_CHAR = '|';
    private const byte COLON = (byte) '|';

    /// <summary>
    /// JSON serializer
    /// </summary>
    public IJsonModelSerializer Serializer { get; }

    /// <summary>
    /// For getting codes by type
    /// </summary>
    private readonly Dictionary<Type, string> _typeCodes = new Dictionary<Type, string>();

    /// <summary>
    /// For getting type by code
    /// </summary>
    private readonly Dictionary<string, Type> _codeTypes = new Dictionary<string, Type>();

    /// <summary>
    /// Finds code by type
    /// </summary>
    public string GetCode(Type type) => _typeCodes[type];

    /// <summary>
    /// Finds type by code
    /// </summary>
    public Type GetType(string code) => _codeTypes[code];

    /// <summary>
    /// Creates new pipe model provider
    /// </summary>
    public PipeModelProvider(IJsonModelSerializer serializer)
    {
        Serializer = serializer;
    }

    /// <summary>
    /// Registers a type into provider
    /// </summary>
    public void Register(Type type)
    {
        ModelTypeAttribute attribute = type.GetCustomAttribute<ModelTypeAttribute>(false);
        string code = attribute != null ? attribute.TypeCode : type.Name;

        _typeCodes.Add(type, code);
        _codeTypes.Add(code, type);
    }

    /// <summary>
    /// Reads websocket message and deserializes model
    /// </summary>
    public object Get(WebSocketMessage message, Type modelType)
    {
        message.Content.Position = 0;
        StreamReader reader = new StreamReader(message.Content);
        string serialized = reader.ReadToEnd();
        int index = serialized.IndexOf(COLON_CHAR);
        if (index < 0)
            return null;

        return Serializer.Deserialize(serialized.Substring(index + 1), modelType);
    }

    /// <summary>
    /// Creates new websocket message and writes the model 
    /// </summary>
    public WebSocketMessage Write(object model)
    {
        Type type = model.GetType();
        if (!_typeCodes.TryGetValue(type, out string code))
        {
            ModelTypeAttribute attr = type.GetCustomAttribute<ModelTypeAttribute>();
            code = attr == null ? type.Name : attr.TypeCode;

            _typeCodes.Add(type, code);
            _codeTypes.Add(code, type);
        }

        string content = code + "|" + Serializer.Serialize(model);
        return WebSocketMessage.FromString(content);
    }

    /// <summary>
    /// Creates new websocket message and writes the model 
    /// </summary>
    public WebSocketMessage Write(string customCode, object model)
    {
        string content = customCode + "|" + Serializer.Serialize(model);
        return WebSocketMessage.FromString(content);
    }

    /// <summary>
    /// Resolves model type from websocket message
    /// </summary>
    public Type Resolve(WebSocketMessage message)
    {
        if (message.OpCode != SocketOpCode.UTF8)
            return null;

        message.Content.Position = 0;
        byte[] buffer = new byte[256];
        int count = message.Content.Read(buffer, 0, buffer.Length);
        if (count == 0)
            return null;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(buffer, 0, count);
        int i = s.IndexOf(COLON);
        if (i < 0)
            return null;

        string typeCode = Encoding.UTF8.GetString(buffer, 0, i);

        Type type;
        bool found = _codeTypes.TryGetValue(typeCode, out type);
        if (!found)
            return null;

        //leave it ready to start reading from beginning of the model itself
        message.Content.Position = i + 1;

        return type;
    }
}