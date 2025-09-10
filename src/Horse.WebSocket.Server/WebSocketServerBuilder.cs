using System;
using System.Collections.Generic;
using Horse.Core.Protocols;
using Horse.Protocols.Http;
using Horse.Server;
using Horse.WebSocket.Protocol;
using Horse.WebSocket.Protocol.Providers;
using Horse.WebSocket.Protocol.Security;
using Horse.WebSocket.Protocol.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Horse.WebSocket.Server;

/// <summary>
/// WebSocket Server builder
/// </summary>
public class WebSocketServerBuilder<TClient> where TClient : IHorseWebSocket
{
    private readonly EncryptorContainer _encryptorContainer = new EncryptorContainer();
    private readonly IServiceCollection _services;
    private ServerOptions _serverOptions = new ServerOptions();
    private bool _statusCodeResponsesDisabled;

    internal ModelWsConnectionHandler Handler { get; private set; }
    internal int Port { get; set; } = 80;

    /// <summary>
    /// Microsoft Dependency Injection Service Collection
    /// </summary>
    public IServiceCollection Services => _services;

    internal WebSocketServerBuilder(IServiceCollection services)
    {
        _services = services;
        Handler = new ModelWsConnectionHandler();
        services.AddSingleton<IWebSocketServerBus>(Handler);
    }

    internal HorseServer Build()
    {
        return Build(new HorseServer(_serverOptions));
    }

    internal HorseServer Build(HorseServer server)
    {
        //we need http protocol is added
        IHorseProtocol http = server.FindProtocol("http");
        if (http == null)
        {
            HttpOptions httpOptions = HttpOptions.CreateDefault();
            httpOptions.UseDefaultStatusCodeResponse = !_statusCodeResponsesDisabled;
            HorseHttpProtocol httpProtocol = new HorseHttpProtocol(server, new WebSocketHttpHandler(), httpOptions);
            server.UseProtocol(httpProtocol);
        }

        HorseWebSocketProtocol protocol = new HorseWebSocketProtocol(server, Handler);
        protocol.EncryptorContainer = _encryptorContainer;

        server.UseProtocol(protocol);
        return server;
    }

    #region Server Options

    /// <summary>
    /// Disables default status code response pages
    /// </summary>
    public void DisableDefaultStatusCodePages()
    {
        _statusCodeResponsesDisabled = true;
    }

    /// <summary>
    /// Implement custom server options
    /// </summary>
    public WebSocketServerBuilder<TClient> UseServerOptions(ServerOptions options)
    {
        _serverOptions = options;
        return this;
    }

    /// <summary>
    /// Configure server options
    /// </summary>
    public WebSocketServerBuilder<TClient> ConfigureServerOptions(Action<ServerOptions> configure)
    {
        _serverOptions = new ServerOptions();
        configure(_serverOptions);
        return this;
    }

    /// <summary>
    /// Changes Web Socket Server port. Default is 80.
    /// </summary>
    public WebSocketServerBuilder<TClient> UsePort(int port)
    {
        Port = port;
        return this;
    }

    /// <summary>
    /// Uses message encryptor
    /// </summary>
    public WebSocketServerBuilder<TClient> UseEncryption<TMessageEncryptor>(Action<TMessageEncryptor> config) where TMessageEncryptor : class, IMessageEncryptor, new()
    {
        TMessageEncryptor encryptor = new TMessageEncryptor();
        config(encryptor);
        _encryptorContainer.SetEncryptor(encryptor);
        return this;
    }

    /// <summary>
    /// Adds authenticator for IWebSocketMessageHandler requests
    /// </summary>
    public WebSocketServerBuilder<TClient> AddAuthenticator(IWebSocketAuthenticator authenticator)
    {
        Handler.Observer.Authenticators.Add(authenticator);
        return this;
    }

    #endregion

    #region Events

    /// <summary>
    /// Action to handle client connections and decide client type
    /// </summary>
    public WebSocketServerBuilder<TClient> OnClientConnected(ConnectedHandler func)
    {
        Handler.ConnectedFunc = func;
        return this;
    }

    /// <summary>
    /// Action to handle client ready status
    /// </summary>
    public WebSocketServerBuilder<TClient> OnClientReady(ClientReadyHandler action)
    {
        Handler.ReadyAction = action;
        return this;
    }

    /// <summary>
    /// Action to handle client disconnections
    /// </summary>
    public WebSocketServerBuilder<TClient> OnClientDisconnected(DisconnectedHandler action)
    {
        Handler.DisconnectedAction = action;
        return this;
    }

    /// <summary>
    /// Action to handle received messages
    /// </summary>
    public WebSocketServerBuilder<TClient> OnMessageReceived(MessageReceivedHandler action)
    {
        Handler.MessageReceivedAction = action;
        return this;
    }

    /// <summary>
    /// Action to handle errors
    /// </summary>
    public WebSocketServerBuilder<TClient> OnError(WebSocketErrorHandler action)
    {
        Handler.ErrorAction = action;
        return this;
    }

    #endregion

    #region Register

    /// <summary>
    /// Uses pipe model provider
    /// </summary>
    public WebSocketServerBuilder<TClient> UsePipeModelProvider(IJsonModelSerializer serializer = null)
    {
        if (serializer == null)
            serializer = new NewtonsoftJsonModelSerializer();

        if (Handler.Observer.HandlersRegistered)
            throw new InvalidOperationException("You must use Use...Provider methods before Add..Handler(s) methods. Change method call order.");

        Handler.Observer.TextProvider = new PipeModelProvider(serializer);

        if (_services != null)
            _services.AddSingleton(Handler.Observer.TextProvider);

        return this;
    }

    /// <summary>
    /// Uses payload model provider
    /// </summary>
    public WebSocketServerBuilder<TClient> UsePayloadModelProvider(IJsonModelSerializer serializer = null)
    {
        if (serializer == null)
            serializer = new NewtonsoftJsonModelSerializer();

        if (Handler.Observer.HandlersRegistered)
            throw new InvalidOperationException("You must use Use...Provider methods before Add..Handler(s) methods. Change method call order.");

        Handler.Observer.TextProvider = new PayloadModelProvider(serializer);

        if (_services != null)
            _services.AddSingleton(Handler.Observer.TextProvider);

        return this;
    }

    /// <summary>
    /// Uses binary model provider. Models must implement IBinaryWebSocketModel interface.
    /// </summary>
    public WebSocketServerBuilder<TClient> UseBinaryModelProvider()
    {
        if (Handler.Observer.HandlersRegistered)
            throw new InvalidOperationException("You must use Use...Provider methods before Add..Handler(s) methods. Change method call order.");

        Handler.Observer.BinaryProvider = new BinaryModelProvider();

        if (_services != null)
            _services.AddSingleton(Handler.Observer.BinaryProvider);

        return this;
    }

    /// <summary>
    /// Uses custom model provider
    /// </summary>
    public WebSocketServerBuilder<TClient> UseModelProvider(IWebSocketModelProvider provider)
    {
        if (Handler.Observer.HandlersRegistered)
            throw new InvalidOperationException("You must use Use...Provider methods before Add..Handler(s) methods. Change method call order.");

        if (provider.Binary)
            Handler.Observer.BinaryProvider = provider;
        else
            Handler.Observer.TextProvider = provider;

        _services?.AddSingleton(Handler.Observer.TextProvider);
        return this;
    }

    /// <summary>
    /// Uses custom model provider
    /// </summary>
    public WebSocketServerBuilder<TClient> UseModelProvider<TWebSocketModelProvider>()
        where TWebSocketModelProvider : IWebSocketModelProvider, new()
    {
        if (Handler.Observer.HandlersRegistered)
            throw new InvalidOperationException("You must use Use...Provider methods before Add..Handler(s) methods. Change method call order.");

        TWebSocketModelProvider provider = new TWebSocketModelProvider();
        if (provider.Binary)
            Handler.Observer.BinaryProvider = provider;
        else
            Handler.Observer.TextProvider = provider;

        _services?.AddSingleton<IWebSocketModelProvider>(provider);
        return this;
    }

    /// <summary>
    /// Registers IWebSocketMessageHandlers.
    /// All handlers must have parameterless constructor.
    /// If you need to inject services, use overloads.
    /// </summary>
    public WebSocketServerBuilder<TClient> AddHandlers(params Type[] assemblyTypes)
    {
        Handler.Observer.RegisterWebSocketHandlers<TClient>(null, assemblyTypes);
        return this;
    }

    /// <summary>
    /// Registers a handler as transient
    /// </summary>
    public WebSocketServerBuilder<TClient> AddTransientHandler(Type handlerType)
    {
        return AddHandler(ServiceLifetime.Transient, handlerType);
    }

    /// <summary>
    /// Registers a handler as scoped
    /// </summary>
    public WebSocketServerBuilder<TClient> AddScopedHandler(Type handlerType)
    {
        return AddHandler(ServiceLifetime.Scoped, handlerType);
    }

    /// <summary>
    /// Registers a handler as singleton
    /// </summary>
    public WebSocketServerBuilder<TClient> AddSingletonHandler(Type handlerType)
    {
        return AddHandler(ServiceLifetime.Singleton, handlerType);
    }


    /// <summary>
    /// Registers handlers as transient
    /// </summary>
    public WebSocketServerBuilder<TClient> AddTransientHandlers(params Type[] assemblyTypes)
    {
        return AddHandlers(ServiceLifetime.Transient, assemblyTypes);
    }

    /// <summary>
    /// Registers handlers as scoped
    /// </summary>
    public WebSocketServerBuilder<TClient> AddScopedHandlers(params Type[] assemblyTypes)
    {
        return AddHandlers(ServiceLifetime.Scoped, assemblyTypes);
    }

    /// <summary>
    /// Registers handlers as singleton
    /// </summary>
    public WebSocketServerBuilder<TClient> AddSingletonHandlers(params Type[] assemblyTypes)
    {
        return AddHandlers(ServiceLifetime.Singleton, assemblyTypes);
    }

    /// <summary>
    /// Registers handlers
    /// </summary>
    private WebSocketServerBuilder<TClient> AddHandler(ServiceLifetime lifetime, Type handlerType)
    {
        if (_services == null)
            throw new ArgumentNullException("ServiceCollection is not attached yet. Use AddBus method before adding handlers.");

        Handler.Observer.RegisterWebSocketHandler(handlerType, typeof(TClient), () => Handler.ServiceProvider);
        RegisterHandler(lifetime, handlerType);

        return this;
    }

    /// <summary>
    /// Registers handlers
    /// </summary>
    private WebSocketServerBuilder<TClient> AddHandlers(ServiceLifetime lifetime, params Type[] assemblyTypes)
    {
        if (_services == null)
            throw new ArgumentNullException("ServiceCollection is not attached yet. Use AddBus method before adding handlers.");

        List<Type> types = Handler.Observer.RegisterWebSocketHandlers<TClient>(() => Handler.ServiceProvider, assemblyTypes);
        foreach (Type type in types)
            RegisterHandler(lifetime, type);

        return this;
    }

    private void RegisterHandler(ServiceLifetime lifetime, Type serviceType)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Transient:
                _services.AddTransient(serviceType);
                break;

            case ServiceLifetime.Singleton:
                _services.AddSingleton(serviceType);
                break;

            case ServiceLifetime.Scoped:
                _services.AddScoped(serviceType);
                break;
        }
    }

    /// <summary>
    /// Gets message bus of websocket server
    /// </summary>
    /// <returns></returns>
    public IWebSocketServerBus GetBus()
    {
        return Handler;
    }

    #endregion
}