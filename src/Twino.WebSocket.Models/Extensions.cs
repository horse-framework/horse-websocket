using System;
using System.Collections.Generic;
using Twino.Ioc;

namespace Twino.WebSocket.Models
{
    /// <summary>
    /// Extension methods for websockets
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Creates new websocket connector and registers IWebSocketBus to the container
        /// </summary>
        public static ITwinoServiceCollection UseWebSocketClient(this ITwinoServiceCollection services, Action<TwinoWebSocketBuilder> config)
        {
            TwinoWebSocketBuilder builder = new TwinoWebSocketBuilder();
            config(builder);

            WebSocketModelConnector connector = builder.Build();

            AddHandlers((IServiceContainer) services, connector, builder);
            services.AddSingleton(connector);
            services.AddSingleton<IWebSocketBus>(connector);

            connector.Run();
            return services;
        }

        private static void AddHandlers(IServiceContainer container, WebSocketModelConnector connector, TwinoWebSocketBuilder builder)
        {
            foreach (Tuple<ImplementationType, Type> pair in builder.AssembyConsumers)
            {
                List<Type> types = connector.Observer.RegisterWebSocketHandlers(type => container.Get(type), pair.Item2);

                foreach (Type type in types)
                    AddHandlerIntoContainer(container, pair.Item1, type);
            }

            foreach (Tuple<ImplementationType, Type, Type> tuple in builder.IndividualConsumers)
            {
                connector.Observer.RegisterWebSocketHandler(tuple.Item2, tuple.Item3, null, type => container.Get(type));
                AddHandlerIntoContainer(container, tuple.Item1, tuple.Item2);
            }
        }

        internal static void AddHandlerIntoContainer(ITwinoServiceCollection container, ImplementationType implementationType, Type consumerType)
        {
            switch (implementationType)
            {
                case ImplementationType.Transient:
                    container.AddTransient(consumerType, consumerType);
                    break;

                case ImplementationType.Scoped:
                    container.AddScoped(consumerType, consumerType);
                    break;

                case ImplementationType.Singleton:
                    container.AddSingleton(consumerType, consumerType);
                    break;
            }
        }
    }
}