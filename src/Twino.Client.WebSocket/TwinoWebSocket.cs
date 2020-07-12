﻿using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Twino.Core;
using Twino.Protocols.Http;
using Twino.Protocols.WebSocket;

namespace Twino.Client.WebSocket
{
    /// <summary>
    /// WebSocket Client class
    /// Can be used directly with event subscriptions
    /// Or can be base class to a derived Client class and provides virtual methods for all events
    /// </summary>
    public class TwinoWebSocket : ClientSocketBase<WebSocketMessage>
    {
        #region Events - Properties

        private static readonly WebSocketWriter _writer = new WebSocketWriter();

        /// <summary>
        /// Key value for the websocket connection
        /// </summary>
        public string WebSocketKey { get; private set; }

        #endregion

        #region Connect

        /// <summary>
        /// Connects to well defined remote host
        /// </summary>
        public override void Connect(DnsInfo host)
        {
            try
            {
                Client = new TcpClient();
                Client.Connect(host.IPAddress, host.Port);
                IsConnected = true;
                IsSsl = host.SSL;

                //creates SSL Stream or Insecure stream
                if (host.SSL)
                {
                    SslStream sslStream = new SslStream(Client.GetStream(), true, CertificateCallback);

                    X509Certificate2Collection certificates = null;
                    if (Certificate != null)
                    {
                        certificates = new X509Certificate2Collection();
                        certificates.Add(Certificate);
                    }

                    sslStream.AuthenticateAsClient(host.Hostname, certificates, false);
                    Stream = sslStream;
                }
                else
                    Stream = Client.GetStream();

                //creates new HTTP Request and sends via Stream
                byte[] request = CreateRequest(host);
                Stream.Write(request, 0, request.Length);

                //Reads the response. Expected response is 101 Switching Protocols (if the server supports web sockets)
                byte[] buffer = new byte[8192];
                int len = Stream.Read(buffer, 0, buffer.Length);

                CheckProtocolResponse(buffer, len);
                Start();
            }
            catch
            {
                Disconnect();
                throw;
            }
        }

        /// <summary>
        /// Connects to well defined remote host
        /// </summary>
        public override async Task ConnectAsync(DnsInfo host)
        {
            try
            {
                Client = new TcpClient();
                await Client.ConnectAsync(host.IPAddress, host.Port);
                IsConnected = true;
                IsSsl = host.SSL;

                //creates SSL Stream or Insecure stream
                if (host.SSL)
                {
                    SslStream sslStream = new SslStream(Client.GetStream(), true, CertificateCallback);

                    X509Certificate2Collection certificates = null;
                    if (Certificate != null)
                    {
                        certificates = new X509Certificate2Collection();
                        certificates.Add(Certificate);
                    }

                    await sslStream.AuthenticateAsClientAsync(host.Hostname, certificates, false);
                    Stream = sslStream;
                }
                else
                    Stream = Client.GetStream();

                //creates new HTTP Request and sends via Stream
                byte[] request = CreateRequest(host);
                await Stream.WriteAsync(request);

                //Reads the response. Expected response is 101 Switching Protocols (if the server supports web sockets)
                byte[] buffer = new byte[8192];
                int len = await Stream.ReadAsync(buffer, 0, buffer.Length);

                CheckProtocolResponse(buffer, len);
                Start();
            }
            catch
            {
                Disconnect();
                throw;
            }
        }

        /// <summary>
        /// Checks if data is a valid protocol message
        /// </summary>
        private void CheckProtocolResponse(byte[] buffer, int length)
        {
            string response = Encoding.UTF8.GetString(buffer, 0, length);

            string first = response.Substring(0, 50).Trim();
            int i1 = first.IndexOf(' ');
            if (i1 < 1)
                throw new InvalidOperationException("Unexpected server response");

            int i2 = first.IndexOf(' ', i1 + 1);
            if (i1 < 0 || i2 < 0 || i2 <= i1)
                throw new InvalidOperationException("Unexpected server response");

            string statusCode = first.Substring(i1, i2 - i1).Trim();
            if (statusCode != "101")
                throw new InvalidOperationException("Connection Error: " + statusCode);

            //Creates HttpRequest class from the response message
            RequestBuilder reader = new RequestBuilder();
            HttpRequest requestResponse = reader.Build(response.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));

            //server must send the web socket accept key for the websocket protocol
            if (!requestResponse.Headers.ContainsKey(HttpHeaders.WEBSOCKET_ACCEPT))
                throw new InvalidOperationException("Handshaking error, server didn't response Sec-WebSocket-Accept");

            string rkey = requestResponse.Headers[HttpHeaders.WEBSOCKET_ACCEPT];

            //check if the key is valid
            using SHA1 sha1 = SHA1.Create();
            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(WebSocketKey + HttpHeaders.WEBSOCKET_GUID));
            string fkey = Convert.ToBase64String(hash);

            if (rkey != fkey)
                throw new InvalidOperationException("Handshaking error, Invalid Key");
        }

        /// <summary>
        /// Starts to read received messages from stream
        /// </summary>
        private void Start()
        {
            //fire connected events and start to read data from the server until disconnected
            Thread thread = new Thread(async () =>
            {
                try
                {
                    while (IsConnected)
                        await Read();
                }
                catch
                {
                    Disconnect();
                }
            });

            thread.IsBackground = true;
            thread.Start();

            OnConnected();
        }

        /// <summary>
        /// Creates HTTP Request for well defined remote host
        /// </summary>
        private byte[] CreateRequest(DnsInfo dns)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(Guid.NewGuid().ToByteArray());
                WebSocketKey = Convert.ToBase64String(hash);
            }

            string request = HttpHeaders.HTTP_GET + " " + dns.Path + " " + HttpHeaders.HTTP_VERSION + "\r\n" +
                             HttpHeaders.Create(HttpHeaders.HOST, dns.Hostname) +
                             HttpHeaders.Create(HttpHeaders.CONNECTION, HttpHeaders.UPGRADE) +
                             HttpHeaders.Create(HttpHeaders.PRAGMA, HttpHeaders.VALUE_NO_CACHE) +
                             HttpHeaders.Create(HttpHeaders.CACHE_CONTROL, HttpHeaders.VALUE_NO_CACHE) +
                             HttpHeaders.Create(HttpHeaders.UPGRADE, HttpHeaders.VALUE_WEBSOCKET) +
                             HttpHeaders.Create(HttpHeaders.WEBSOCKET_VERSION, HttpHeaders.VALUE_WEBSOCKET_VERSION) +
                             HttpHeaders.Create(HttpHeaders.ACCEPT_ENCODING, HttpHeaders.VALUE_GZIP_DEFLATE_BR) +
                             HttpHeaders.Create(HttpHeaders.ACCEPT_LANGUAGE, HttpHeaders.VALUE_ACCEPT_EN) +
                             HttpHeaders.Create(HttpHeaders.WEBSOCKET_KEY, WebSocketKey) +
                             HttpHeaders.Create(HttpHeaders.WEBSOCKET_EXTENSIONS, HttpHeaders.VALUE_WEBSOCKET_EXTENSIONS);

            lock (Data)
                foreach (var kv in Data.Properties)
                    request += HttpHeaders.Create(kv.Key, kv.Value);

            request += "\r\n";
            return Encoding.UTF8.GetBytes(request);
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Reads next message from stream
        /// </summary>
        protected override async Task Read()
        {
            WebSocketReader reader = new WebSocketReader();
            WebSocketMessage message = await reader.Read(Stream);
            if (message == null)
            {
                Disconnect();
                return;
            }

            KeepAlive();
            switch (message.OpCode)
            {
                case SocketOpCode.Binary:
                case SocketOpCode.UTF8:
                    SetOnMessageReceived(message);
                    break;

                case SocketOpCode.Terminate:
                    Disconnect();
                    break;

                case SocketOpCode.Pong:
                    break;

                case SocketOpCode.Ping:
                    Pong();
                    break;
            }
        }

        /// <summary>
        /// Sends websocket ping message
        /// </summary>
        public sealed override void Ping()
        {
            Send(Protocols.WebSocket.PredefinedMessages.PING);
        }

        /// <summary>
        /// Sends websocket pong message
        /// </summary>
        public sealed override void Pong()
        {
            Send(Protocols.WebSocket.PredefinedMessages.PONG);
        }

        /// <summary>
        /// Sends a string websocket message
        /// </summary>
        public bool Send(string message)
        {
            byte[] data = _writer.Create(message).Result;
            return Send(data);
        }

        /// <summary>
        /// Sends a string websocket message
        /// </summary>
        public async Task<bool> SendAsync(string message)
        {
            byte[] data = await _writer.Create(message);
            return await SendAsync(data);
        }

        #endregion
    }
}