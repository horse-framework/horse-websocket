using System.Threading.Tasks;
using Horse.Client.Connectors;
using Horse.Client.WebSocket.Connectors;
using Horse.Protocols.WebSocket;

namespace Horse.Client.WebSocket
{
    /// <summary>
    /// Extension methods for websocket clients
    /// </summary>
    public static class ConnectorExtensions
    {
        private static readonly WebSocketWriter _writer = new WebSocketWriter();

        /// <summary>
        /// Sends string websocket message
        /// </summary>
        public static bool Send(this WsStickyConnector connector, string message)
        {
            return SendInternal(connector, message);
        }

        /// <summary>
        /// Sends string websocket message
        /// </summary>
        public static async Task<bool> SendAsync(this WsStickyConnector connector, string message)
        {
            return await SendInternalAsync(connector, message);
        }

        /// <summary>
        /// Sends string websocket message
        /// </summary>
        public static bool Send(this WsAbsoluteConnector connector, string message)
        {
            return SendInternal(connector, message);
        }

        /// <summary>
        /// Sends string websocket message
        /// </summary>
        public static async Task<bool> SendAsync(this WsAbsoluteConnector connector, string message)
        {
            return await SendInternalAsync(connector, message);
        }

        /// <summary>
        /// Sends string websocket message
        /// </summary>
        public static bool Send(this WsNecessityConnector connector, string message)
        {
            return SendInternal(connector, message);
        }

        /// <summary>
        /// Sends string websocket message
        /// </summary>
        public static async Task<bool> SendAsync(this WsNecessityConnector connector, string message)
        {
            return await SendInternalAsync(connector, message);
        }

        /// <summary>
        /// Sends string websocket message
        /// </summary>
        public static bool Send(this WsSingleMessageConnector connector, string message)
        {
            return SendInternal(connector, message);
        }

        /// <summary>
        /// Sends string websocket message
        /// </summary>
        public static async Task<bool> SendAsync(this WsSingleMessageConnector connector, string message)
        {
            return await SendInternalAsync(connector, message);
        }

        /// <summary>
        /// Sends string websocket message
        /// </summary>
        private static bool SendInternal(IConnector<HorseWebSocket, WebSocketMessage> connector, string message)
        {
            byte[] data = _writer.Create(WebSocketMessage.FromString(message)).Result;
            return connector.Send(data);
        }

        /// <summary>
        /// Sends string websocket message
        /// </summary>
        private static async Task<bool> SendInternalAsync(IConnector<HorseWebSocket, WebSocketMessage> connector, string message)
        {
            byte[] data = await _writer.Create(WebSocketMessage.FromString(message));
            HorseWebSocket client = connector.GetClient();

            if (client != null && client.IsConnected)
                return await client.SendAsync(data);

            return false;
        }
    }
}