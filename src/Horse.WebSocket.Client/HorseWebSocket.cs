using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Horse.WebSocket.Protocol;
using Horse.WebSocket.Protocol.Providers;
using Horse.WebSocket.Protocol.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

public class HorseWebSocket<TIdentifier> : HorseWebSocket
{
}

public class HorseWebSocket : IDisposable
{
    #region Properties - Fields

    public string RemoteHost { get; set; }

    public HorseWebSocketConnection Connection { get; private set; }

    public WebSocketMessageObserver Observer { get; private set; }

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
    /// Internal connected event
    /// </summary>
    internal Action<HorseWebSocket> ConnectedAction { get; set; }

    /// <summary>
    /// Internal disconnected event
    /// </summary>
    internal Action<HorseWebSocket> DisconnectedAction { get; set; }

    /// <summary>
    /// Internal message received event
    /// </summary>
    internal Action<WebSocketMessage> MessageReceivedAction { get; set; }

    /// <summary>
    /// Internal error event
    /// </summary>
    internal Action<Exception> ErrorAction { get; set; }

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

    public HorseWebSocket()
    {
        Observer = new WebSocketMessageObserver(new PipeModelProvider(new SystemJsonModelSerializer()), OnErrorOccured);
    }

    public void Dispose()
    {
        _autoReconnect = false;
        _reconnectTimer?.Dispose();
        _reconnectTimer = null;
        Connection?.Disconnect();
        Connection = null;
    }

    private void OnErrorOccured(Exception exception)
    {
        ErrorAction?.Invoke(exception);
        Error?.Invoke(this, exception);
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

    public Task ConnectAsync(string host)
    {
        RemoteHost = host;
        UpdateReconnectTimer();
        return ConnectAsync();
    }

    public async Task<bool> ConnectAsync()
    {
        try
        {
            Connection = new HorseWebSocketConnection();
            await Connection.ConnectAsync(RemoteHost);
            return true;
        }
        catch (Exception e)
        {
            OnErrorOccured(e);
            return false;
        }
    }

    public void Disconnect()
    {
        SetAutoReconnect(false);

        if (Connection != null)
        {
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

    public void AddHandlerTransient<TMessageHandler, TModel>() where TMessageHandler : IWebSocketMessageHandler<TModel>
    {
        if (_services == null)
        {
            throw new NotSupportedException("Transient handlers are support with Microsoft.Dependency.Injection library. Call UseServices method first");
        }

        Observer.RegisterWebSocketHandler(typeof(TMessageHandler), () => _provider);
        _services.AddTransient(typeof(TMessageHandler));
    }

    public void AddHandlerScoped<TMessageHandler, TModel>() where TMessageHandler : IWebSocketMessageHandler<TModel>
    {
        if (_services == null)
            throw new NotSupportedException("Scoped handlers are support with Microsoft.Dependency.Injection library. Call UseServices method first");

        Observer.RegisterWebSocketHandler(typeof(TMessageHandler), () => _provider);
        _services.AddScoped(typeof(TMessageHandler));
    }

    public void AddHandlerSingleton<TMessageHandler, TModel>() where TMessageHandler : class, IWebSocketMessageHandler<TModel>
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

            TMessageHandler instance = (TMessageHandler) Activator.CreateInstance(handlerType);
            Observer.RegisterWebSocketHandler(typeof(TMessageHandler), typeof(TModel), instance, null);
            return;
        }

        Observer.RegisterWebSocketHandler(typeof(TMessageHandler), () => _provider);
        _services.AddSingleton(typeof(TMessageHandler));
    }

    public void AddHandlerSingleton<TMessageHandler, TModel>(TMessageHandler instance) where TMessageHandler : class, IWebSocketMessageHandler<TModel>
    {
        if (_services == null)
        {
            Observer.RegisterWebSocketHandler(typeof(TMessageHandler), typeof(TModel), instance, null);
            return;
        }

        Observer.RegisterWebSocketHandler(typeof(TMessageHandler), () => _provider);
        _services.AddSingleton(instance);
    }

    public void AddHandlersTransient(params Type[] assemblyTypes)
    {
        if (_services == null)
        {
            throw new NotSupportedException("Transient handlers are support with Microsoft.Dependency.Injection library. Call UseServices method first");
        }

        List<Type> types = Observer.RegisterWebSocketHandlers(() => _provider, assemblyTypes);
        foreach (Type type in types)
            _services.AddTransient(type);
    }

    public void AddHandlersScoped(params Type[] assemblyTypes)
    {
        if (_services == null)
            throw new NotSupportedException("Scoped handlers are support with Microsoft.Dependency.Injection library. Call UseServices method first");

        List<Type> types = Observer.RegisterWebSocketHandlers(() => _provider, assemblyTypes);

        foreach (Type type in types)
            _services.AddScoped(type);
    }

    public void AddHandlersSingleton(params Type[] assemblyTypes)
    {
        if (_services == null)
        {
            Observer.RegisterWebSocketHandlers(null, assemblyTypes);
            return;
        }

        List<Type> types = Observer.RegisterWebSocketHandlers(() => _provider, assemblyTypes);

        foreach (Type type in types)
            _services.AddSingleton(type);
    }

    #endregion

    /// <summary>
    /// Sends a model to websocket server
    /// </summary>
    public Task<bool> SendAsync<TModel>(TModel model)
    {
        WebSocketMessage message = Observer.Provider.Write(model);
        return Connection.SendAsync(message);
    }

    #region Model Providers

    /// <summary>
    /// Uses custom model provider
    /// </summary>
    public void UseCustomModelProvider<TProvider>()
        where TProvider : IWebSocketModelProvider, new()
    {
        Observer.Provider = new TProvider();
    }

    /// <summary>
    /// Uses custom model provider
    /// </summary>
    public void UseCustomModelProvider(IWebSocketModelProvider provider)
    {
        Observer.Provider = provider;
    }

    /// <summary>
    /// Uses pipe model provider.
    /// Models are sent in payload property in JSON model model-type|{ name: "foo" }
    /// </summary>
    public void UsePipeModelProvider(IJsonModelSerializer serializer = null)
    {
        if (serializer == null)
            serializer = new NewtonsoftJsonModelSerializer();

        Observer.Provider = new PayloadModelProvider(serializer);
    }

    /// <summary>
    /// Uses payload model provider.
    /// Models are sent in payload property in JSON model { type: "model-type", payload: your_model }
    /// </summary>
    public void UsePayloadModelProvider(IJsonModelSerializer serializer = null)
    {
        if (serializer == null)
            serializer = new NewtonsoftJsonModelSerializer();

        Observer.Provider = new PayloadModelProvider(serializer);
    }

    #endregion
}