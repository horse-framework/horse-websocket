# Twino WebSocket

Twino WebSocket includes websocket server and websocket client. Websocket servers runs on Twino Server. You can implement websocket server with Twino MVC and Twino MQ too, or alone.

## NuGet Packages

**[Twino WebSocket Server Library](https://www.nuget.org/packages/Twino.Protocols.WebSocket)**<br>
**[Twino WebSocket Client](https://www.nuget.org/packages/Twino.Client.WebSocket)**<br>
**[Model Serialize Helper Library](https://www.nuget.org/packages/Twino.SerializableModel)**<br>

#### Basic WebSocket Server Example

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


#### Basic WebSocket Client Example


     TwinoWebSocket client = new TwinoWebSocket();
     client.MessageReceived += (c, m) => Console.WriteLine("# " + m);
     client.Connected += c => Console.WriteLine("Connected");
     client.Disconnected += c => Console.WriteLine("Disconnected");
     client.Connect("wss://echo.websocket.org");

     while (true)
     {
         string s = Console.ReadLine();
         client.Send(s);
     }
