# Twino WebSocket

[![NuGet](https://img.shields.io/nuget/v/Twino.Client.WebSocket?label=client%20nuget)](https://www.nuget.org/packages/Twino.Client.WebSocket)
[![NuGet](https://img.shields.io/nuget/v/Twino.Protocols.WebSocket?label=server%20nuget)](https://www.nuget.org/packages/Twino.Protocols.WebSocket)
[![NuGet](https://img.shields.io/nuget/v/Twino.WebSocket.Models?label=extensions%20nuget)](https://www.nuget.org/packages/Twino.WebSocket.Models)

Twino WebSocket includes websocket server and websocket client. Websocket servers runs on Twino Server. You can implement websocket server with Twino MVC and Twino MQ too, or alone.

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

## Thanks

Thanks to JetBrains for a open source license to use on this project.

[![jetbrains](https://user-images.githubusercontent.com/21208762/90192662-10043700-ddcc-11ea-9533-c43b99801d56.png)](https://www.jetbrains.com/?from=twino-framework)
