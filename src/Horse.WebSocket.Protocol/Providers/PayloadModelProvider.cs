using System;
using System.Collections.Generic;
using System.Reflection;
using Horse.WebSocket.Protocol.Serialization;

namespace Horse.WebSocket.Protocol.Providers;

/// <summary>
/// Payload provider by class name.
/// Serialize data seems like; {type:"ModelA",payload:{name:"foo",no:123}}
/// </summary>
public class PayloadModelProvider : ISerializableProvider
{
    /// <inheritdoc />
    public void WarmUp(params Assembly[] assemblies)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// JSON serializer
    /// </summary>
    public IJsonModelSerializer Serializer { get; }

    /// <summary>
    /// For getting codes by type
    /// </summary>
    private readonly Dictionary<Type, string> _typeCodes = new();

    /// <summary>
    /// For getting type by code
    /// </summary>
    private readonly Dictionary<string, Type> _codeTypes = new();

    /// <summary>
    /// Finds code by type
    /// </summary>
    public string GetCode(Type type) => _typeCodes[type];

    /// <summary>
    /// Finds type by code
    /// </summary>
    public Type GetType(string code) => _codeTypes[code];

    /// <summary>
    /// Creates new payload model provider
    /// </summary>
    public PayloadModelProvider(IJsonModelSerializer serializer)
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

        Type openGeneric = typeof(PayloadFrame<>);
        Type genericType = openGeneric.MakeGenericType(modelType);

        dynamic model = Serializer.Deserialize(message.ToString(), genericType);
        return model.Payload;
    }

    /// <summary>
    /// Creates new websocket message and writes the model 
    /// </summary>
    public WebSocketMessage Write(object model)
    {
        Type type = model.GetType();
        string code;
        if (_typeCodes.ContainsKey(type))
            code = _typeCodes[type];
        else
        {
            ModelTypeAttribute attr = type.GetCustomAttribute<ModelTypeAttribute>();
            code = attr == null ? type.Name : attr.TypeCode;

            _typeCodes.Add(type, code);
            _codeTypes.Add(code, type);
        }

        var obj = new
        {
            type = code,
            payload = model
        };

        string content = Serializer.Serialize(obj);
        return WebSocketMessage.FromString(content);
    }

    /// <summary>
    /// Creates new websocket message and writes the model 
    /// </summary>
    public WebSocketMessage Write(string customCode, object model)
    {
        var obj = new
        {
            type = customCode,
            payload = model
        };

        string content = Serializer.Serialize(obj);
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

        PayloadResolve resolve = (PayloadResolve) Serializer.Deserialize(message.ToString(), typeof(PayloadResolve));
        if (resolve == null || string.IsNullOrEmpty(resolve.Type))
            return null;
        Type type;
        bool found = _codeTypes.TryGetValue(resolve.Type, out type);
        if (!found)
            return null;

        return type;
    }
}