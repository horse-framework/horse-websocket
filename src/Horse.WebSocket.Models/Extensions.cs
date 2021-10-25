using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Horse.WebSocket.Models
{
    /// <summary>
    /// Extension methods for websockets
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Creates new websocket connector and registers IWebSocketBus to the MSDI container.
        /// </summary>
        public static IServiceCollection AddWebSocketClient(this IServiceCollection services, Action<HorseWebSocketBuilder> config)
        {
            HorseWebSocketBuilder builder = new HorseWebSocketBuilder();
            config(builder);

            WebSocketModelConnector connector = builder.Build();
            connector.Builder = builder;

            AddMSDIHandlers(services, connector, builder);
            services.AddSingleton(connector);
            services.AddSingleton<IWebSocketBus>(connector);

            return services;
        }

        /// <summary>
        /// Creates new websocket connector and registers IWebSocketBus to the MSDI container.
        /// </summary>
        public static IServiceCollection AddWebSocketClient<TIdentifer>(this IServiceCollection services, Action<HorseWebSocketBuilder> config)
        {
            HorseWebSocketBuilder<TIdentifer> builder = new HorseWebSocketBuilder<TIdentifer>();
            config(builder);

            WebSocketModelConnector<TIdentifer> connector = (WebSocketModelConnector<TIdentifer>) builder.Build();

            AddMSDIHandlers(services, connector, builder);
            services.AddSingleton(connector);
            services.AddSingleton<IWebSocketBus<TIdentifer>>(connector);

            return services;
        }

        private static void AddMSDIHandlers(IServiceCollection services, WebSocketModelConnector connector, HorseWebSocketBuilder builder)
        {
            foreach (Tuple<ServiceLifetime, Type> pair in builder.AssembyConsumers)
            {
                List<Type> types = connector.Observer.ResolveWebSocketHandlerTypes(pair.Item2);
                foreach (Type type in types)
                    AddHandlerIntoMSDI(services, pair.Item1, type);
            }

            foreach (Tuple<ServiceLifetime, Type, Type> tuple in builder.IndividualConsumers)
                AddHandlerIntoMSDI(services, tuple.Item1, tuple.Item2);
        }

        internal static void AddHandlerIntoMSDI(IServiceCollection services, ServiceLifetime lifetime, Type consumerType)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Transient:
                    services.AddTransient(consumerType, consumerType);
                    break;

                case ServiceLifetime.Scoped:
                    services.AddScoped(consumerType, consumerType);
                    break;

                case ServiceLifetime.Singleton:
                    services.AddSingleton(consumerType, consumerType);
                    break;
            }
        }

        /// <summary>
        /// Uses Websocket Client and connects to the server.
        /// This method should be used for only MSDI Provider.
        /// </summary>
        public static IServiceProvider UseWebSocketClient(this IServiceProvider provider)
        {
            WebSocketModelConnector connector = provider.GetService<WebSocketModelConnector>();
            connector.ServiceProvider = provider;
            
            foreach (Tuple<ServiceLifetime, Type> pair in connector.Builder.AssembyConsumers)
                connector.Observer.RegisterWebSocketHandlers(() => connector.ServiceProvider, pair.Item2);

            foreach (Tuple<ServiceLifetime, Type, Type> tuple in connector.Builder.IndividualConsumers)
                connector.Observer.RegisterWebSocketHandler(tuple.Item2, tuple.Item3, null, () => connector.ServiceProvider);

            connector.Builder = null;
            
            connector.Run();
            return provider;
        }

        /// <summary>
        /// Uses Websocket Client and connects to the server.
        /// This method should be used for only MSDI Provider.
        /// </summary>
        public static IServiceProvider UseWebSocketClient<TIdentier>(this IServiceProvider provider)
        {
            WebSocketModelConnector connector = provider.GetService<WebSocketModelConnector<TIdentier>>();
            connector.ServiceProvider = provider;
            
            foreach (Tuple<ServiceLifetime, Type> pair in connector.Builder.AssembyConsumers)
                connector.Observer.RegisterWebSocketHandlers(() => connector.ServiceProvider, pair.Item2);

            foreach (Tuple<ServiceLifetime, Type, Type> tuple in connector.Builder.IndividualConsumers)
                connector.Observer.RegisterWebSocketHandler(tuple.Item2, tuple.Item3, null, () => connector.ServiceProvider);

            connector.Builder = null;
            
            connector.Run();
            return provider;
        }
    }
}