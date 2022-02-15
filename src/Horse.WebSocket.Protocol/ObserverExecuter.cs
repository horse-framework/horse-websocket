using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Horse.WebSocket.Protocol;

internal class ObserverExecuter<TModel> : ObserverExecuter
{
    private readonly IWebSocketMessageHandler<TModel> _instance;
    private readonly Func<IServiceProvider> _providerFactory;
    private readonly Func<Action<Exception>> _errorFactory;
    private readonly Type _consumerType;

    public ObserverExecuter(Type consumerType,
        IWebSocketMessageHandler<TModel> instance,
        Func<IServiceProvider> providerFactory,
        Func<Action<Exception>> errorFactory)
    {
        _consumerType = consumerType;
        _instance = instance;
        _providerFactory = providerFactory;
        _errorFactory = errorFactory;
    }

    public override async Task Execute(object model, WebSocketMessage message, IHorseWebSocket client)
    {
        IWebSocketMessageHandler<TModel> handler = null;

        try
        {
            if (_instance != null)
            {
                handler = _instance;

                await handler.Handle((TModel) model, message, client);
            }
            else if (_providerFactory != null)
            {
                IServiceProvider provider = _providerFactory();
                using IServiceScope scope = provider.CreateScope();
                handler = (IWebSocketMessageHandler<TModel>) scope.ServiceProvider.GetService(_consumerType);

                await handler.Handle((TModel) model, message, client);
            }
        }
        catch (Exception e)
        {
            Action<Exception> errorAction = null;
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
                if (errorAction != null)
                    errorAction(e);

                return;
            }

            try
            {
                await handler.OnError(e, (TModel) model, message, client);

                if (errorAction != null)
                    errorAction(e);
            }
            catch (Exception e2)
            {
                if (errorAction != null)
                    errorAction(e2);
            }
        }
    }
}

internal abstract class ObserverExecuter
{
    public abstract Task Execute(object model, WebSocketMessage message, IHorseWebSocket client);
}