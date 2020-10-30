using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Twino.Ioc;

namespace Twino.WebSocket.Models
{
    /// <summary>
    /// Extension methods for websockets
    /// </summary>
    public static class Extensions
    {
        #region Twino IOC

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

        /// <summary>
        /// Creates new websocket connector and registers IWebSocketBus to the container
        /// </summary>
        public static ITwinoServiceCollection UseWebSocketClient<TIdentifier>(this ITwinoServiceCollection services, Action<TwinoWebSocketBuilder> config)
        {
            TwinoWebSocketBuilder<TIdentifier> builder = new TwinoWebSocketBuilder<TIdentifier>();
            config(builder);

            WebSocketModelConnector<TIdentifier> connector = (WebSocketModelConnector<TIdentifier>) builder.Build();

            AddHandlers((IServiceContainer) services, connector, builder);
            services.AddSingleton(connector);
            services.AddSingleton<IWebSocketBus<TIdentifier>>(connector);

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

        #endregion

        #region MS DI

        /// <summary>
        /// Creates new websocket connector and registers IWebSocketBus to the MSDI container.
        /// </summary>
        public static IServiceCollection AddWebSocketClient(this IServiceCollection services, Action<TwinoWebSocketBuilder> config)
        {
            TwinoWebSocketBuilder builder = new TwinoWebSocketBuilder();
            config(builder);

            WebSocketModelConnector connector = builder.Build();

            AddMSDIHandlers(services, connector, builder);
            services.AddSingleton(connector);
            services.AddSingleton<IWebSocketBus>(connector);

            return services;
        }

        /// <summary>
        /// Creates new websocket connector and registers IWebSocketBus to the MSDI container.
        /// </summary>
        public static IServiceCollection AddWebSocketClient<TIdentifer>(this IServiceCollection services, Action<TwinoWebSocketBuilder> config)
        {
            TwinoWebSocketBuilder<TIdentifer> builder = new TwinoWebSocketBuilder<TIdentifer>();
            config(builder);

            WebSocketModelConnector<TIdentifer> connector = (WebSocketModelConnector<TIdentifer>) builder.Build();

            AddMSDIHandlers(services, connector, builder);
            services.AddSingleton(connector);
            services.AddSingleton<IWebSocketBus<TIdentifer>>(connector);

            return services;
        }

        private static void AddMSDIHandlers(IServiceCollection services, WebSocketModelConnector connector, TwinoWebSocketBuilder builder)
        {
            foreach (Tuple<ImplementationType, Type> pair in builder.AssembyConsumers)
            {
                List<Type> types = connector.Observer.RegisterWebSocketHandlers(type => connector.ServiceProvider.GetService(type), pair.Item2);

                foreach (Type type in types)
                    AddHandlerIntoMSDI(services, pair.Item1, type);
            }

            foreach (Tuple<ImplementationType, Type, Type> tuple in builder.IndividualConsumers)
            {
                connector.Observer.RegisterWebSocketHandler(tuple.Item2, tuple.Item3, null, type => connector.ServiceProvider.GetService(type));
                AddHandlerIntoMSDI(services, tuple.Item1, tuple.Item2);
            }
        }

        internal static void AddHandlerIntoMSDI(IServiceCollection services, ImplementationType implementationType, Type consumerType)
        {
            switch (implementationType)
            {
                case ImplementationType.Transient:
                    services.AddTransient(consumerType, consumerType);
                    break;

                case ImplementationType.Scoped:
                    services.AddScoped(consumerType, consumerType);
                    break;

                case ImplementationType.Singleton:
                    services.AddSingleton(consumerType, consumerType);
                    break;
            }
        }

        /// <summary>
        /// Uses Websocket Client and connects to the server.
        /// This method should be used for only MSDI Provider.
        /// For Twino IOC, use overload with ITwinoServiceCollection parameter
        /// </summary>
        public static IServiceProvider UseWebSocketClient(this IServiceProvider provider)
        {
            WebSocketModelConnector connector = provider.GetService<WebSocketModelConnector>();
            connector.ServiceProvider = provider;
            connector.Run();
            return provider;
        }

        /// <summary>
        /// Uses Websocket Client and connects to the server.
        /// This method should be used for only MSDI Provider.
        /// For Twino IOC, use overload with ITwinoServiceCollection parameter
        /// </summary>
        public static IServiceProvider UseWebSocketClient<TIdentier>(this IServiceProvider provider)
        {
            WebSocketModelConnector connector = provider.GetService<WebSocketModelConnector<TIdentier>>();
            connector.ServiceProvider = provider;
            connector.Run();
            return provider;
        }

        #endregion
    }
}