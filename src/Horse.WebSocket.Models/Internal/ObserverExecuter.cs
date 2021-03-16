using System;
using System.Threading.Tasks;
using Horse.Protocols.WebSocket;

namespace Horse.WebSocket.Models.Internal
{
    internal class ObserverExecuter<TModel> : ObserverExecuter
    {
        private readonly IWebSocketModelProvider _provider;
        private readonly IWebSocketMessageHandler<TModel> _instance;
        private readonly Func<Type, object> _factory;
        private readonly Func<Action<Exception>> _errorFactory;
        private readonly Type _consumerType;

        public ObserverExecuter(Type consumerType,
                                IWebSocketModelProvider provider,
                                IWebSocketMessageHandler<TModel> instance,
                                Func<Type, object> factory,
                                Func<Action<Exception>> errorFactory)
        {
            _consumerType = consumerType;
            _provider = provider;
            _instance = instance;
            _factory = factory;
            _errorFactory = errorFactory;
        }

        public override async Task Execute(object model, WebSocketMessage message, IHorseWebSocket client)
        {
            IWebSocketMessageHandler<TModel> handler = null;

            try
            {
                if (_instance != null)
                    handler = _instance;
                else if (_factory != null)
                    handler = (IWebSocketMessageHandler<TModel>) _factory(_consumerType);
                else
                    return;
                
                await handler.Handle((TModel) model, message, client);
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
}