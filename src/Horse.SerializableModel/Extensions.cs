﻿using System.IO;
using System.Text;
using System.Threading.Tasks;
using Horse.Core;
using Horse.Protocols.WebSocket;
using Horse.SerializableModel.Serialization;

namespace Horse.SerializableModel
{
    /// <summary>
    /// Extension methods for horse serializable models
    /// </summary>
    public static class Extensions
    {
        private static readonly WebSocketWriter _writer = new WebSocketWriter();
        private static readonly HorseModelWriter _twriter = new HorseModelWriter();

        /// <summary>
        /// Creates web socket message from model and sends to specified client.
        /// HorseModelWriter is used as IModelWriter. Use overload to customize.
        /// </summary>
        public static void Send<TModel>(this SocketBase socket, TModel model) where TModel : ISerializableModel
        {
            Send(socket, model, _twriter);
        }

        /// <summary>
        /// Creates web socket message from model and sends to specified client
        /// </summary>
        public static void Send<TModel>(this SocketBase socket, TModel model, IModelWriter writer) where TModel : ISerializableModel
        {
            WebSocketMessage message = new WebSocketMessage
            {
                OpCode = SocketOpCode.UTF8,
                Content = new MemoryStream(Encoding.UTF8.GetBytes(writer.Serialize(model)))
            };

            socket.Send(_writer.Create(message).Result);
        }


        /// <summary>
        /// Creates web socket message from model and sends to specified client.
        /// HorseModelWriter is used as IModelWriter. Use overload to customize.
        /// </summary>
        public static async Task SendAsync<TModel>(this SocketBase socket, TModel model) where TModel : ISerializableModel
        {
            await SendAsync(socket, model, _twriter);
        }

        /// <summary>
        /// Creates web socket message from model and sends to specified client
        /// </summary>
        public static async Task SendAsync<TModel>(this SocketBase socket, TModel model, IModelWriter writer) where TModel : ISerializableModel
        {
            WebSocketMessage message = new WebSocketMessage
            {
                OpCode = SocketOpCode.UTF8,
                Content = new MemoryStream(Encoding.UTF8.GetBytes(writer.Serialize(model)))
            };

            await socket.SendAsync(await _writer.Create(message));
        }

        /// <summary>
        /// Uses default HorseModelWriter and WebSocketWriter classes.
        /// Creates websocket protocol byte array data of serialized model string. 
        /// </summary>
        public static byte[] ToWebSocketUTF8Data(this ISerializableModel model)
        {
            WebSocketMessage message = new WebSocketMessage
            {
                OpCode = SocketOpCode.UTF8,
                Content = new MemoryStream(Encoding.UTF8.GetBytes(_twriter.Serialize(model)))
            };

            return _writer.Create(message).Result;
        }

        /// <summary>
        /// Uses default HorseModelWriter and WebSocketWriter classes.
        /// Creates websocket protocol byte array data of serialized model string. 
        /// </summary>
        public static async Task<byte[]> ToWebSocketUTF8DataAsync(this ISerializableModel model)
        {
            WebSocketMessage message = new WebSocketMessage
            {
                OpCode = SocketOpCode.UTF8,
                Content = new MemoryStream(Encoding.UTF8.GetBytes(_twriter.Serialize(model)))
            };

            return await _writer.Create(message);
        }
    }
}