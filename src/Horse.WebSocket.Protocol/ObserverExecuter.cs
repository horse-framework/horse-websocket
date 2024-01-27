using System;
using System.Reflection;
using System.Threading.Tasks;
using Horse.WebSocket.Protocol.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Horse.WebSocket.Protocol;

internal class ObserverExecuter<TModel, TClient> : ObserverExecuter where TClient : IHorseWebSocket
{
    private readonly IWebSocketMessageHandler<TModel, TClient> _instance;
    private readonly Func<IServiceProvider> _providerFactory;
    private readonly Func<WebSocketErrorHandler> _errorFactory;
    private readonly Type _consumerType;

    public ObserverExecuter(Type consumerType,
        IWebSocketMessageHandler<TModel, TClient> instance,
        Func<IServiceProvider> providerFactory,
        Func<WebSocketErrorHandler> errorFactory)
    {
        _consumerType = consumerType;
        _instance = instance;
        _providerFactory = providerFactory;
        _errorFactory = errorFactory;
    }

    private async Task<bool> CheckAuthentication(IHorseWebSocket client, WebSocketMessage message, Type modelType, Type handlerType)
    {
        if (!IsAuthenticationRequired.HasValue)
        {
            AuthenticateAttribute attribute = handlerType.GetCustomAttribute<AuthenticateAttribute>();
            IsAuthenticationRequired = attribute != null;
        }

        if (!IsAuthenticationRequired.Value)
            return true;

        WsServerSocket socket = (WsServerSocket) client;
        if (!socket.IsAuthenticated)
            return false;

        foreach (IWebSocketAuthenticator authenticator in Observer.Authenticators)
        {
            if (!await authenticator.Authenticate(socket, message, modelType, handlerType))
                return false;
        }

        return true;
    }

    public override async Task Execute(object model, WebSocketMessage message, IHorseWebSocket client)
    {
        IWebSocketMessageHandler<TModel, TClient> handler = null;

        try
        {
            TModel typedModel = (TModel) model;
            if (_instance != null)
            {
                handler = _instance;

                if (!await CheckAuthentication(client, message, typeof(TModel), _instance.GetType()))
                {
                    await handler.OnUnauthenticated(typedModel, message, (TClient) client);
                    return;
                }

                await handler.Handle(typedModel, message, (TClient) client);
            }
            else if (_providerFactory != null)
            {
                IServiceProvider provider = _providerFactory();
                using IServiceScope scope = provider.CreateScope();
                handler = (IWebSocketMessageHandler<TModel, TClient>) scope.ServiceProvider.GetService(_consumerType);

                if (!await CheckAuthentication(client, message, typeof(TModel), handler.GetType()))
                {
                    await handler.OnUnauthenticated(typedModel, message, (TClient) client);
                    return;
                }

                await handler.Handle(typedModel, message, (TClient) client);
            }
        }
        catch (Exception e)
        {
            WebSocketErrorHandler errorAction = null;
            if (_errorFactory != null)
            {
                try
                {
                    errorAction = _errorFactory();
                }
                catch
                {
                }
            }

            if (handler == null)
            {
                errorAction?.Invoke(e, message, client);
                return;
            }

            try
            {
                await handler.OnError(e, (TModel) model, message, (TClient) client);
                errorAction?.Invoke(e, message, client);
            }
            catch (Exception e2)
            {
                errorAction?.Invoke(e2, message, client);
            }
        }
    }
}

internal abstract class ObserverExecuter
{
    internal WebSocketMessageObserver Observer { get; set; }
    internal bool? IsAuthenticationRequired { get; set; }

    public abstract Task Execute(object model, WebSocketMessage message, IHorseWebSocket client);
}