using System;
using Horse.Client.WebSocket;
using Horse.Client.WebSocket.Connectors;
using Horse.Core;

namespace Sample.Connectors
{
    class Program
    {
        static void Main(string[] args)
        {
            WsStickyConnector connector = new WsStickyConnector(TimeSpan.FromMilliseconds(20));
            connector.AddHost("ws://127.0.0.1");
            connector.Connected += Connector_Connected;
            connector.Disconnected += Connector_Disconnected;
            connector.Run();

            while (true)
            {
                connector.Send("Hello world!");
                Console.ReadLine();
            }
        }

        private static void Connector_Disconnected(SocketBase client)
        {
            Console.WriteLine("dc");
        }

        private static void Connector_Connected(SocketBase client)
        {
            Console.WriteLine("c");
        }
    }
}