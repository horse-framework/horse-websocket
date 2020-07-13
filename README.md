### Basic WebSocket Server Example

Basic WebSocket Server creation example

    class Program
    {
        static void Main(string[] args)
        {
            TwinoServer server = new TwinoServer();
            server.UseWebSockets((socket, message) => { Console.WriteLine($"Received: {message}"); });
	    
	    //or advanced with IProtocolConnectionHandler<WebSocketMessage> implementation
            //server.UseWebSockets(new ServerWsHandler());
            server.Start(80);
            
            //optional
            _server.Server.BlockWhileRunning();
        }
    }
