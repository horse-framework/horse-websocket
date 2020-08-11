using System;
using System.Threading.Tasks;
using Twino.Protocols.WebSocket;

namespace Twino.WebSocket.Models.Internal
{
    internal class ObserverExecuter<TModel> : ObserverExecuter
    {
        private readonly IWebSocketModelProvider _provider;
        private readonly IWebSocketMessageHandler<TModel> _instance;
        private readonly Func<Type, object> _factory;
        private readonly Action<Exception> _error;
        private readonly Type _consumerType;

        public ObserverExecuter(Type consumerType,
                                IWebSocketModelProvider provider,
                                IWebSocketMessageHandler<TModel> instance,
                                Func<Type, object> factory,
                                Action<Exception> error)
        {
            _consumerType = consumerType;
            _provider = provider;
            _instance = instance;
            _factory = factory;
            _error = error;
        }

        public override async Task Execute(object model, WebSocketMessage message, ITwinoWebSocket client)
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
                if (handler == null)
                {
                    if (_error != null)
                        _error(e);

                    return;
                }

                try
                {
                    await handler.OnError(e, (TModel) model, message, client);

                    if (_error != null)
                        _error(e);
                }
                catch (Exception e2)
                {
                    if (_error != null)
                        _error(e2);
                }
            }
        }
    }

    internal abstract class ObserverExecuter
    {
        public abstract Task Execute(object model, WebSocketMessage message, ITwinoWebSocket client);
    }
}