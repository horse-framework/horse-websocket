using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Horse.Core;
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
public class WebSocketServerBuilder
{
    private IMessageEncryptor _encryptor;
    private IServiceCollection _services;
    private ServerOptions _serverOptions = new ServerOptions();

    internal ModelWsConnectionHandler Handler { get; private set; }
    internal int Port { get; set; } = 80;

    internal WebSocketServerBuilder()
    {
        Handler = new ModelWsConnectionHandler();
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
            HorseHttpProtocol httpProtocol = new HorseHttpProtocol(server, new WebSocketHttpHandler(), httpOptions);
            server.UseProtocol(httpProtocol);
        }

        HorseWebSocketProtocol protocol = new HorseWebSocketProtocol(server, Handler);
        protocol.Encryptor = _encryptor;

        server.UseProtocol(protocol);
        return server;
    }

    #region Server Options

    /// <summary>
    /// Implement custom server options
    /// </summary>
    public WebSocketServerBuilder UseServerOptions(ServerOptions options)
    {
        _serverOptions = options;
        return this;
    }

    /// <summary>
    /// Configure server options
    /// </summary>
    public WebSocketServerBuilder ConfigureServerOptions(Action<ServerOptions> configure)
    {
        _serverOptions = new ServerOptions();
        configure(_serverOptions);
        return this;
    }

    /// <summary>
    /// Changes Web Socket Server port. Default is 80.
    /// </summary>
    public WebSocketServerBuilder UsePort(int port)
    {
        Port = port;
        return this;
    }

    /// <summary>
    /// Uses message encryptor
    /// </summary>
    public WebSocketServerBuilder UseEncryption<TMessageEncryptor>(Action<TMessageEncryptor> config) where TMessageEncryptor : class, IMessageEncryptor, new()
    {
        TMessageEncryptor encryptor = new TMessageEncryptor();
        _encryptor = encryptor;
        config(encryptor);
        return this;
    }

    #endregion

    #region Events

    /// <summary>
    /// Action to handle client connections and decide client type
    /// </summary>
    public WebSocketServerBuilder OnClientConnected(Func<IConnectionInfo, ConnectionData, Task<WsServerSocket>> func)
    {
        Handler.ConnectedFunc = func;
        return this;
    }

    /// <summary>
    /// Action to handle client ready status
    /// </summary>
    public WebSocketServerBuilder OnClientReady(Func<WsServerSocket, Task> action)
    {
        Handler.ReadyAction = action;
        return this;
    }

    /// <summary>
    /// Action to handle client disconnections
    /// </summary>
    public WebSocketServerBuilder OnClientDisconnected(Func<WsServerSocket, Task> action)
    {
        Handler.DisconnectedAction = action;
        return this;
    }

    /// <summary>
    /// Action to handle received messages
    /// </summary>
    public WebSocketServerBuilder OnMessageReceived(Func<WebSocketMessage, WsServerSocket, Task> action)
    {
        Handler.MessageReceivedAction = action;
        return this;
    }

    /// <summary>
    /// Action to handle errors
    /// </summary>
    public WebSocketServerBuilder OnError(Action<Exception> action)
    {
        Handler.ErrorAction = action;
        return this;
    }

    #endregion

    #region Register

    /// <summary>
    /// Uses pipe model provider
    /// </summary>
    public WebSocketServerBuilder UsePipeModelProvider(IJsonModelSerializer serializer = null)
    {
        if (serializer == null)
            serializer = new NewtonsoftJsonModelSerializer();

        if (Handler.Observer.HandlersRegistered)
            throw new InvalidOperationException("You must use Use...Provider methods before Add..Handler(s) methods. Change method call order.");

        Handler.Observer.Provider = new PipeModelProvider(serializer);

        if (_services != null)
            _services.AddSingleton(Handler.Observer.Provider);

        return this;
    }

    /// <summary>
    /// Uses payload model provider
    /// </summary>
    public WebSocketServerBuilder UsePayloadModelProvider(IJsonModelSerializer serializer = null)
    {
        if (serializer == null)
            serializer = new NewtonsoftJsonModelSerializer();

        if (Handler.Observer.HandlersRegistered)
            throw new InvalidOperationException("You must use Use...Provider methods before Add..Handler(s) methods. Change method call order.");

        Handler.Observer.Provider = new PayloadModelProvider(serializer);

        if (_services != null)
            _services.AddSingleton(Handler.Observer.Provider);

        return this;
    }

    /// <summary>
    /// Uses binary model provider. Models must implement IBinaryWebSocketModel interface.
    /// </summary>
    public WebSocketServerBuilder UseBinaryModelProvider()
    {
        if (Handler.Observer.HandlersRegistered)
            throw new InvalidOperationException("You must use Use...Provider methods before Add..Handler(s) methods. Change method call order.");

        Handler.Observer.Provider = new BinaryModelProvider();

        if (_services != null)
            _services.AddSingleton(Handler.Observer.Provider);

        return this;
    }

    /// <summary>
    /// Uses custom model provider
    /// </summary>
    public WebSocketServerBuilder UseModelProvider(IWebSocketModelProvider provider)
    {
        if (Handler.Observer.HandlersRegistered)
            throw new InvalidOperationException("You must use Use...Provider methods before Add..Handler(s) methods. Change method call order.");

        Handler.Observer.Provider = provider;

        if (_services != null)
            _services.AddSingleton(Handler.Observer.Provider);

        return this;
    }

    /// <summary>
    /// Uses custom model provider
    /// </summary>
    public WebSocketServerBuilder UseModelProvider<TWebSocketModelProvider>()
        where TWebSocketModelProvider : IWebSocketModelProvider, new()
    {
        if (Handler.Observer.HandlersRegistered)
            throw new InvalidOperationException("You must use Use...Provider methods before Add..Handler(s) methods. Change method call order.");

        Handler.Observer.Provider = new TWebSocketModelProvider();

        if (_services != null)
            _services.AddSingleton(Handler.Observer.Provider);

        return this;
    }

    /// <summary>
    /// Registers IWebSocketMessageHandlers.
    /// All handlers must have parameterless constructor.
    /// If you need to inject services, use overloads.
    /// </summary>
    public WebSocketServerBuilder AddHandlers(params Type[] assemblyTypes)
    {
        Handler.Observer.RegisterWebSocketHandlers(null, assemblyTypes);
        return this;
    }

    /// <summary>
    /// Registers a handler as transient
    /// </summary>
    public WebSocketServerBuilder AddTransientHandler(Type handlerType)
    {
        return AddHandler(ServiceLifetime.Transient, handlerType);
    }

    /// <summary>
    /// Registers a handler as scoped
    /// </summary>
    public WebSocketServerBuilder AddScopedHandler(Type handlerType)
    {
        return AddHandler(ServiceLifetime.Scoped, handlerType);
    }

    /// <summary>
    /// Registers a handler as singleton
    /// </summary>
    public WebSocketServerBuilder AddSingletonHandler(Type handlerType)
    {
        return AddHandler(ServiceLifetime.Singleton, handlerType);
    }


    /// <summary>
    /// Registers handlers as transient
    /// </summary>
    public WebSocketServerBuilder AddTransientHandlers(params Type[] assemblyTypes)
    {
        return AddHandlers(ServiceLifetime.Transient, assemblyTypes);
    }

    /// <summary>
    /// Registers handlers as scoped
    /// </summary>
    public WebSocketServerBuilder AddScopedHandlers(params Type[] assemblyTypes)
    {
        return AddHandlers(ServiceLifetime.Scoped, assemblyTypes);
    }

    /// <summary>
    /// Registers handlers as singleton
    /// </summary>
    public WebSocketServerBuilder AddSingletonHandlers(params Type[] assemblyTypes)
    {
        return AddHandlers(ServiceLifetime.Singleton, assemblyTypes);
    }

    /// <summary>
    /// Registers handlers
    /// </summary>
    private WebSocketServerBuilder AddHandler(ServiceLifetime lifetime, Type handlerType)
    {
        if (_services == null)
            throw new ArgumentNullException("ServiceCollection is not attached yet. Use AddBus method before adding handlers.");

        Handler.Observer.RegisterWebSocketHandler(handlerType, () => Handler.ServiceProvider);
        RegisterHandler(lifetime, handlerType);

        return this;
    }

    /// <summary>
    /// Registers handlers
    /// </summary>
    private WebSocketServerBuilder AddHandlers(ServiceLifetime lifetime, params Type[] assemblyTypes)
    {
        if (_services == null)
            throw new ArgumentNullException("ServiceCollection is not attached yet. Use AddBus method before adding handlers.");

        List<Type> types = Handler.Observer.RegisterWebSocketHandlers(() => Handler.ServiceProvider, assemblyTypes);
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
    public WebSocketServerBuilder UseMSDI(IServiceCollection services)
    {
        if (services.All(x => x.ServiceType != typeof(IWebSocketServerBus)))
            services.AddSingleton<IWebSocketServerBus>(Handler);

        _services = services;

        return this;
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