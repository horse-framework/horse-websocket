using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Horse.Core;
using Horse.WebSocket.Protocol;
using Horse.WebSocket.Protocol.Providers;
using Horse.WebSocket.Protocol.Security;
using Horse.WebSocket.Protocol.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Horse.WebSocket.Client;

/// <summary>
/// Delegate for HorseClient OnMessageReceived event
/// </summary>
public delegate void ClientMessageReceivedHandler(HorseWebSocket client, WebSocketMessage message);

/// <summary>
/// Delegate for HorseClient Connected and Disconnected events
/// </summary>
public delegate void ClientConnectionHandler(HorseWebSocket client);

/// <summary>
/// Delete for HorseClient Error event
/// </summary>
public delegate void ClientErrorHandler(HorseWebSocket client, Exception exception, WebSocketMessage message = null);

/// <summary>
/// Horse WebSocket Client.
/// TIdentifier is for multiple implementation on dependency injection libraries 
/// </summary>
public class HorseWebSocket<TIdentifier> : HorseWebSocket
{
}

/// <summary>
/// Horse WebSocket Client
/// </summary>
public class HorseWebSocket : IDisposable
{
    #region Properties - Fields

    /// <summary>
    /// Remote Host with protocol name
    /// </summary>
    public string RemoteHost { get; set; }

    /// <summary>
    /// Tag is for end-user.
    /// You can set different values for your different clients.
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    /// Base connection object for websocket protocol
    /// </summary>
    public HorseWebSocketConnection Connection { get; private set; }

    /// <summary>
    /// Observer object for model implementations
    /// </summary>
    public WebSocketMessageObserver Observer { get; }

    /// <summary>
    /// Message Encryptor implementation
    /// </summary>
    public EncryptorContainer EncryptorContainer { get; set; } = new EncryptorContainer();

    /// <summary>
    /// The waiting time before reconnecting, after disconnection.
    /// Default value ise 3 seconds.
    /// </summary>
    public TimeSpan AutoReconnectDelay
    {
        get => _autoReconnectDelay;
        set
        {
            _autoReconnectDelay = value;
            if (_reconnectTimer is not null)
                throw new InvalidOperationException("Reconnect wait time cannot modify when client is connected.");

            UpdateReconnectTimer();
        }
    }

    /// <summary>
    /// If true, client tries reconnect to the sever if disconnected.
    /// </summary>
    public bool AutoReconnect => _autoReconnect;

    private IServiceCollection _services;
    private IServiceProvider _provider;
    private TimeSpan _autoReconnectDelay = TimeSpan.FromSeconds(3);
    private Timer _reconnectTimer;
    private bool _autoReconnect = true;
    private readonly List<KeyValuePair<string, string>> _headers = new List<KeyValuePair<string, string>>();

    #endregion

    #region Event Properties

    /// <summary>
    /// Returns true if connected
    /// </summary>
    public bool IsConnected => Connection != null && Connection.IsConnected;

    /// <summary>
    /// Triggered when client receives a message
    /// </summary>
    public event ClientMessageReceivedHandler MessageReceived;

    /// <summary>
    /// Triggered when client connects to the server
    /// </summary>
    public event ClientConnectionHandler Connected;

    /// <summary>
    /// Triggered when client disconnects from the server
    /// </summary>
    public event ClientConnectionHandler Disconnected;

    /// <summary>
    /// Triggered when an exception is thrown in client operations
    /// </summary>
    public event ClientErrorHandler Error;

    #endregion

    #region Init - Connect - Destroy

    /// <summary>
    /// Creates new Horse WebSocket Client
    /// </summary>
    public HorseWebSocket()
    {
        Observer = new WebSocketMessageObserver(new PipeModelProvider(new SystemJsonModelSerializer()), new BinaryModelProvider(), OnErrorOccured);
    }

    /// <summary>
    /// Disposes all resources of WebSocket Client.
    /// If client is still connected, disconnects from the server.
    /// </summary>
    public void Dispose()
    {
        _autoReconnect = false;
        _reconnectTimer?.Dispose();
        _reconnectTimer = null;
        Connection?.Disconnect();
        Connection = null;
    }

    private void OnErrorOccured(Exception exception, WebSocketMessage message, IHorseWebSocket client)
    {
        Error?.Invoke(this, exception);
    }

    private void OnConnected(SocketBase socket)
    {
        Connected?.Invoke(this);
    }

    private void OnDisconnected(SocketBase socket)
    {
        Disconnected?.Invoke(this);
    }

    private void OnMessageReceived(ClientSocketBase<WebSocketMessage> socket, WebSocketMessage message)
    {
        if (EncryptorContainer.HasAnyEncryptor && message.Content.Length > 0)
        {
            byte encryptorId = (byte)message.Content.ReadByte();
            IMessageEncryptor encryptor = EncryptorContainer.GetEncryptor(encryptorId);
            byte[] array = new byte[message.Content.Length - 1];

            int left = array.Length;
            while (left > 0)
            {
                int read = message.Content.Read(array, 0, left);
                left -= read;
            }

            message.Content = new MemoryStream(encryptor.DecryptData(array));
        }

        _ = Observer.Read(message, Connection);
        MessageReceived?.Invoke(this, message);
    }

    private void UpdateReconnectTimer()
    {
        if (_autoReconnectDelay == TimeSpan.Zero)
            return;

        if (_reconnectTimer == null)
        {
            int ms = Convert.ToInt32(_autoReconnectDelay.TotalMilliseconds);
            _reconnectTimer = new Timer(_ =>
            {
                if (!_autoReconnect || IsConnected)
                    return;

                if (Connection != null && !Connection.IsConnected)
                    Connection.Disconnect();

                _ = ConnectAsync();
            }, null, ms, ms);
        }
    }

    private void SetAutoReconnect(bool value)
    {
        _autoReconnect = value;
        if (value)
        {
            if (_reconnectTimer != null) return;

            UpdateReconnectTimer();
        }
        else
        {
            if (_reconnectTimer != null)
            {
                _reconnectTimer.Dispose();
                _reconnectTimer = null;
            }
        }
    }

    /// <summary>
    /// Connects to specified remote host
    /// </summary>
    public Task ConnectAsync(string host)
    {
        RemoteHost = host;
        UpdateReconnectTimer();
        return ConnectAsync();
    }

    /// <summary>
    /// Connects to predefined remote host
    /// </summary>
    public async Task<bool> ConnectAsync()
    {
        try
        {
            Connection = new HorseWebSocketConnection(this);
            Connection.EncryptorContainer = EncryptorContainer;
            foreach (KeyValuePair<string, string> pair in _headers)
                Connection.Data.Properties.Add(pair.Key, pair.Value);

            Connection.Connected += OnConnected;
            Connection.Disconnected += OnDisconnected;
            Connection.MessageReceived += OnMessageReceived;
            await Connection.ConnectAsync(RemoteHost);
            return true;
        }
        catch (Exception e)
        {
            OnErrorOccured(e, null, null);
            Connection.Connected -= OnConnected;
            Connection.Disconnected -= OnDisconnected;
            Connection.MessageReceived -= OnMessageReceived;
            return false;
        }
    }

    /// <summary>
    /// Disconnects from the server and disabled auto reconnection if it's enabled.
    /// </summary>
    public void Disconnect()
    {
        SetAutoReconnect(false);

        if (Connection != null)
        {
            Connection.Connected -= OnConnected;
            Connection.Disconnected -= OnDisconnected;
            Connection.MessageReceived -= OnMessageReceived;
            Connection.Disconnect();
            Connection = null;
        }
    }

    #endregion

    #region Configuration Actions

    /// <summary>
    /// Adds a request header for HTTP protocol
    /// </summary>
    public void AddRequestHeader(string key, string value)
    {
        _headers.Add(new KeyValuePair<string, string>(key, value));
    }

    /// <summary>
    /// Clears all defined request headers
    /// </summary>
    public void ClearRequestHeaders()
    {
        _headers.Clear();
    }

    /// <summary>
    /// Sets reconnect interval for connector
    /// </summary>
    public void SetReconnectInterval(TimeSpan reconnectInterval)
    {
        _autoReconnectDelay = reconnectInterval;
    }

    #endregion

    #region MSDI Implementation

    /// <summary>
    /// Uses a service collection for model handlers.
    /// If UseProvider used after that, implemented provider will be used instead of this collection.
    /// If UseProvider is not used after that, a new provider will be created from this collection on first use
    /// </summary>
    public void UseServices(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Uses a service provider for model handlers.
    /// If UseServices is used before this, it will be overridden.
    /// </summary>
    public void UseProvider(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <summary>
    /// Gets implemented service provider.
    /// If there is no implemented service provider, returns null
    /// </summary>
    public IServiceProvider GetServiceProvider()
    {
        if (_provider != null)
            return _provider;

        if (_services != null)
            _provider = _services.BuildServiceProvider();

        return _provider;
    }

    #endregion

    #region Add Handlers

    /// <summary>
    /// Adds model handler with transient lifetime
    /// </summary>
    public void AddHandlerTransient<TMessageHandler, TModel, TClient>()
        where TMessageHandler : IWebSocketMessageHandler<TModel, TClient>
        where TClient : IHorseWebSocket
    {
        if (_services == null)
        {
            throw new NotSupportedException("Transient handlers are support with Microsoft.Dependency.Injection library. Call UseServices method first");
        }

        Observer.RegisterWebSocketHandler(typeof(TMessageHandler), typeof(TModel), typeof(HorseWebSocketConnection), null, () => _provider);
        _services.AddTransient(typeof(TMessageHandler));
    }

    /// <summary>
    /// Adds model handler with scoped lifetime
    /// </summary>
    public void AddHandlerScoped<TMessageHandler, TModel, TClient>()
        where TMessageHandler : IWebSocketMessageHandler<TModel, TClient>
        where TClient : IHorseWebSocket
    {
        if (_services == null)
            throw new NotSupportedException("Scoped handlers are support with Microsoft.Dependency.Injection library. Call UseServices method first");

        Observer.RegisterWebSocketHandler(typeof(TMessageHandler), typeof(TModel), typeof(HorseWebSocketConnection), null, () => _provider);
        _services.AddScoped(typeof(TMessageHandler));
    }

    /// <summary>
    /// Adds model handler with singleton lifetime
    /// </summary>
    public void AddHandlerSingleton<TMessageHandler, TModel, TClient>()
        where TMessageHandler : class, IWebSocketMessageHandler<TModel, TClient>
        where TClient : IHorseWebSocket
    {
        if (_services == null)
        {
            Type handlerType = typeof(TMessageHandler);
            ConstructorInfo parameterlessCtor = handlerType.GetConstructors().FirstOrDefault(x => x.GetParameters().Length == 0);

            if (parameterlessCtor == null)
            {
                throw new NotSupportedException($"Microsoft.Dependency.Injection is not implemented. " +
                                                $"{handlerType.FullName} must have parameterless constructor. " +
                                                $"Or you can pass singleton instance of handler.");
            }

            TMessageHandler instance = (TMessageHandler)Activator.CreateInstance(handlerType);
            Observer.RegisterWebSocketHandler(typeof(TMessageHandler), typeof(TModel), typeof(HorseWebSocketConnection), instance, null);
            return;
        }

        Observer.RegisterWebSocketHandler(typeof(TMessageHandler), typeof(TModel), typeof(HorseWebSocketConnection), null, () => _provider);
        _services.AddSingleton(typeof(TMessageHandler));
    }

    /// <summary>
    /// Adds model handler with singleton lifetime
    /// </summary>
    public void AddHandlerSingleton<TMessageHandler, TModel, TClient>(TMessageHandler instance)
        where TMessageHandler : class, IWebSocketMessageHandler<TModel, TClient>
        where TClient : IHorseWebSocket
    {
        if (_services == null)
        {
            Observer.RegisterWebSocketHandler(typeof(TMessageHandler), typeof(TModel), typeof(TClient), instance, null);
            return;
        }

        Observer.RegisterWebSocketHandler(typeof(TMessageHandler), typeof(TModel), typeof(TClient), null, () => _provider);
        _services.AddSingleton(instance);
    }

    /// <summary>
    /// Adds model handler with transient lifetime
    /// </summary>
    public void AddHandlerTransient<TMessageHandler, TModel>()
        where TMessageHandler : IWebSocketMessageHandler<TModel>
    {
        if (_services == null)
        {
            throw new NotSupportedException("Transient handlers are support with Microsoft.Dependency.Injection library. Call UseServices method first");
        }

        Observer.RegisterWebSocketHandler(typeof(TMessageHandler), typeof(TModel), typeof(HorseWebSocketConnection), null, () => _provider);
        _services.AddTransient(typeof(TMessageHandler));
    }

    /// <summary>
    /// Adds model handler with scoped lifetime
    /// </summary>
    public void AddHandlerScoped<TMessageHandler, TModel>()
        where TMessageHandler : IWebSocketMessageHandler<TModel>
    {
        if (_services == null)
            throw new NotSupportedException("Scoped handlers are support with Microsoft.Dependency.Injection library. Call UseServices method first");

        Observer.RegisterWebSocketHandler(typeof(TMessageHandler), typeof(TModel), typeof(HorseWebSocketConnection), null, () => _provider);
        _services.AddScoped(typeof(TMessageHandler));
    }

    /// <summary>
    /// Adds model handler with singleton lifetime
    /// </summary>
    public void AddHandlerSingleton<TMessageHandler, TModel>()
        where TMessageHandler : class, IWebSocketMessageHandler<TModel>
    {
        if (_services == null)
        {
            Type handlerType = typeof(TMessageHandler);
            ConstructorInfo parameterlessCtor = handlerType.GetConstructors().FirstOrDefault(x => x.GetParameters().Length == 0);

            if (parameterlessCtor == null)
            {
                throw new NotSupportedException($"Microsoft.Dependency.Injection is not implemented. " +
                                                $"{handlerType.FullName} must have parameterless constructor. " +
                                                $"Or you can pass singleton instance of handler.");
            }

            TMessageHandler instance = (TMessageHandler)Activator.CreateInstance(handlerType);
            Observer.RegisterWebSocketHandler(typeof(TMessageHandler), typeof(TModel), typeof(HorseWebSocketConnection), instance, null);
            return;
        }

        Observer.RegisterWebSocketHandler(typeof(TMessageHandler), typeof(TModel), typeof(HorseWebSocketConnection), null, () => _provider);
        _services.AddSingleton(typeof(TMessageHandler));
    }

    /// <summary>
    /// Adds model handler with singleton lifetime
    /// </summary>
    public void AddHandlerSingleton<TMessageHandler, TModel>(TMessageHandler instance)
        where TMessageHandler : class, IWebSocketMessageHandler<TModel>
    {
        if (_services == null)
        {
            Observer.RegisterWebSocketHandler(typeof(TMessageHandler), typeof(TModel), typeof(HorseWebSocketConnection), instance, null);
            return;
        }

        Observer.RegisterWebSocketHandler(typeof(TMessageHandler), typeof(TModel), typeof(HorseWebSocketConnection), null, () => _provider);
        _services.AddSingleton(instance);
    }

    /// <summary>
    /// Adds model handlers with transient lifetime in specified assemblies
    /// </summary>
    public void AddHandlersTransient(params Type[] assemblyTypes)
    {
        if (_services == null)
        {
            throw new NotSupportedException("Transient handlers are support with Microsoft.Dependency.Injection library. Call UseServices method first");
        }

        List<Type> types = Observer.RegisterWebSocketHandlers<HorseWebSocketConnection>(() => _provider, assemblyTypes);
        foreach (Type type in types)
            _services.AddTransient(type);
    }

    /// <summary>
    /// Adds model handlers with scoped lifetime in specified assemblies
    /// </summary>
    public void AddHandlersScoped<TClient>(params Type[] assemblyTypes) where TClient : IHorseWebSocket
    {
        if (_services == null)
            throw new NotSupportedException("Scoped handlers are support with Microsoft.Dependency.Injection library. Call UseServices method first");

        List<Type> types = Observer.RegisterWebSocketHandlers<HorseWebSocketConnection>(() => _provider, assemblyTypes);

        foreach (Type type in types)
            _services.AddScoped(type);
    }

    /// <summary>
    /// Adds model handlers with singleton lifetime in specified assemblies
    /// </summary>
    public void AddHandlersSingleton<TClient>(params Type[] assemblyTypes) where TClient : IHorseWebSocket
    {
        if (_services == null)
        {
            Observer.RegisterWebSocketHandlers<TClient>(null, assemblyTypes);
            return;
        }

        List<Type> types = Observer.RegisterWebSocketHandlers<TClient>(() => _provider, assemblyTypes);

        foreach (Type type in types)
            _services.AddSingleton(type);
    }

    #endregion

    /// <summary>
    /// Sends a model to websocket server
    /// </summary>
    public Task<bool> SendAsync<TModel>(TModel model)
    {
        IWebSocketModelProvider provider = Observer.BinaryProvider != null && model is IBinaryWebSocketModel
            ? Observer.BinaryProvider
            : Observer.TextProvider;

        WebSocketMessage message = provider.Write(model);
        return Connection.SendAsync(message);
    }

    /// <summary>
    /// Sends a model to websocket server
    /// </summary>
    public Task<bool> SendAsync<TModel>(TModel model, bool binary)
    {
        IWebSocketModelProvider provider = binary
            ? Observer.BinaryProvider
            : Observer.TextProvider;
        
        WebSocketMessage message = provider.Write(model);
        return Connection.SendAsync(message);
    }

    #region Model Providers

    /// <summary>
    /// Uses custom model provider
    /// </summary>
    public void UseCustomModelProvider<TProvider>()
        where TProvider : IWebSocketModelProvider, new()
    {
        TProvider provider = new TProvider();
        if (provider.Binary)
            Observer.BinaryProvider = provider;
        else
            Observer.TextProvider = provider;
    }

    /// <summary>
    /// Uses binary model provider
    /// </summary>
    public void UseBinaryModelProvider()
    {
        Observer.BinaryProvider = new BinaryModelProvider();
    }

    /// <summary>
    /// Uses custom model provider
    /// </summary>
    public void UseCustomModelProvider(IWebSocketModelProvider provider)
    {
        if (provider.Binary)
            Observer.BinaryProvider = provider;
        else
            Observer.TextProvider = provider;
    }

    /// <summary>
    /// Uses pipe model provider.
    /// Models are sent in payload property in JSON model model-type|{ name: "foo" }
    /// </summary>
    public void UsePipeModelProvider(IJsonModelSerializer serializer = null)
    {
        if (serializer == null)
            serializer = new NewtonsoftJsonModelSerializer();

        Observer.TextProvider = new PipeModelProvider(serializer);
    }

    /// <summary>
    /// Uses payload model provider.
    /// Models are sent in payload property in JSON model { type: "model-type", payload: your_model }
    /// </summary>
    public void UsePayloadModelProvider(IJsonModelSerializer serializer = null)
    {
        if (serializer == null)
            serializer = new NewtonsoftJsonModelSerializer();

        Observer.TextProvider = new PayloadModelProvider(serializer);
    }

    #endregion
}