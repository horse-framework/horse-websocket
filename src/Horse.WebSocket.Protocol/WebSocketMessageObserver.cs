using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Horse.WebSocket.Client")]
[assembly: InternalsVisibleTo("Horse.WebSocket.Server")]

namespace Horse.WebSocket.Protocol;

/// <summary>
/// Observer reads all received messages over websocket.
/// Executes handler methods if models are registered.
/// </summary>
public class WebSocketMessageObserver
{
    private readonly Dictionary<Type, ObserverExecuter> _executers = new();
    internal Action<Exception> ErrorAction { get; set; }

    /// <summary>
    /// Returns true if at least one handler is registered
    /// </summary>
    internal bool HandlersRegistered { get; private set; }

    /// <summary>
    /// Model provider for websocket
    /// </summary>
    public IWebSocketModelProvider Provider { get; internal set; }

    /// <summary>
    /// Creates new websocket message observer
    /// </summary>
    public WebSocketMessageObserver(IWebSocketModelProvider provider, Action<Exception> errorAction)
    {
        Provider = provider;
        ErrorAction = errorAction;
    }

    /// <summary>
    /// Reads websocket message over network and process it
    /// </summary>
    public Task Read(WebSocketMessage message, IHorseWebSocket client)
    {
        try
        {
            Type type = Provider.Resolve(message);
            if (type == null)
                return Task.CompletedTask;

            object model = Provider.Get(message, type);
            ObserverExecuter executer = _executers[type];
            return executer.Execute(model, message, client);
        }
        catch (Exception e)
        {
            if (ErrorAction != null)
                ErrorAction(e);

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Resolves handle types
    /// </summary>
    internal List<Type> ResolveWebSocketHandlerTypes(params Type[] assemblyTypes)
    {
        List<Type> items = new List<Type>();
        Type openQueueGeneric = typeof(IWebSocketMessageHandler<>);

        foreach (Type assemblyType in assemblyTypes)
        {
            foreach (Type type in assemblyType.Assembly.GetTypes())
            {
                Type[] interfaceTypes = type.GetInterfaces();
                foreach (Type interfaceType in interfaceTypes)
                {
                    if (!interfaceType.IsGenericType)
                        continue;

                    Type generic = interfaceType.GetGenericTypeDefinition();
                    if (openQueueGeneric.IsAssignableFrom(generic))
                        items.Add(type);
                }
            }
        }

        return items;
    }

    /// <summary>
    /// Registers all handlers in assemblies of the types
    /// </summary>
    public List<Type> RegisterWebSocketHandlers(Func<IServiceProvider> providerFactory, params Type[] assemblyTypes)
    {
        List<Type> items = new List<Type>();
        Type openQueueGeneric = typeof(IWebSocketMessageHandler<>);

        foreach (Type assemblyType in assemblyTypes)
        {
            foreach (Type type in assemblyType.Assembly.GetTypes())
            {
                Type[] interfaceTypes = type.GetInterfaces();
                foreach (Type interfaceType in interfaceTypes)
                {
                    if (!interfaceType.IsGenericType)
                        continue;

                    Type generic = interfaceType.GetGenericTypeDefinition();
                    if (openQueueGeneric.IsAssignableFrom(generic))
                    {
                        Type[] genericArgs = interfaceType.GetGenericArguments();

                        object instance = null;
                        if (providerFactory == null)
                            instance = Activator.CreateInstance(type);

                        RegisterWebSocketHandler(type, genericArgs[0], instance, providerFactory);
                        items.Add(type);
                    }
                }
            }
        }

        return items;
    }

    /// <summary>
    /// Registers a websocket message handler
    /// </summary>
    public void RegisterWebSocketHandler<TObserver, TModel>()
        where TObserver : IWebSocketMessageHandler<TModel>, new()
    {
        RegisterWebSocketHandler(typeof(TObserver), typeof(TModel), new TObserver(), null);
    }

    /// <summary>
    /// Registers a websocket message handler
    /// </summary>
    public void RegisterWebSocketHandler<TModel>(IWebSocketMessageHandler<TModel> messageHandler)
    {
        RegisterWebSocketHandler(messageHandler.GetType(), typeof(TModel), messageHandler, null);
    }

    /// <summary>
    /// Registers a websocket message handler
    /// </summary>
    public void RegisterWebSocketHandler(Type observerType, Func<IServiceProvider> providerFactory)
    {
        RegisterWebSocketHandler(observerType, observerType, null, providerFactory);
    }

    /// <summary>
    /// Registers a websocket message handler
    /// </summary>
    internal void RegisterWebSocketHandler(Type observerType, Type modelType, object instance, Func<IServiceProvider> providerFactory)
    {
        Func<Action<Exception>> errorFactory = () => ErrorAction;
        Type executerType = typeof(ObserverExecuter<>).MakeGenericType(modelType);
        ObserverExecuter executer = (ObserverExecuter) Activator.CreateInstance(executerType,
            observerType,
            instance,
            providerFactory,
            errorFactory);
        Provider.Register(modelType);
        _executers.Add(modelType, executer);
        HandlersRegistered = true;
    }
}