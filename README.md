# Horse WebSocket

[![NuGet](https://img.shields.io/nuget/v/Horse.Client.WebSocket?label=client%20nuget)](https://www.nuget.org/packages/Horse.Client.WebSocket)
[![NuGet](https://img.shields.io/nuget/v/Horse.Protocols.WebSocket?label=server%20nuget)](https://www.nuget.org/packages/Horse.Protocols.WebSocket)
[![NuGet](https://img.shields.io/nuget/v/Horse.WebSocket.Models?label=extensions%20nuget)](https://www.nuget.org/packages/Horse.WebSocket.Models)

Horse WebSocket includes websocket server and websocket client. Websocket servers runs on Horse Server. You can implement websocket server with Horse MVC and/or Horse MQ, or alone.

#### Basic WebSocket Server Example

    class Program
    {
        static void Main(string[] args)
        {
            HorseServer server = new HorseServer();
            server.UseWebSockets((socket, message) => { Console.WriteLine($"Received: {message}"); });
	    
	          //or advanced with IProtocolConnectionHandler<WebSocketMessage> implementation
            
            //server.UseWebSockets(new ServerWsHandler());
            server.Run(80);
        }
    }


#### Basic WebSocket Client Example


     HorseWebSocket client = new HorseWebSocket();
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

Thanks to JetBrains for open source license to use on this project.

[![jetbrains](https://user-images.githubusercontent.com/21208762/90192662-10043700-ddcc-11ea-9533-c43b99801d56.png)](https://www.jetbrains.com/?from=twino-framework)
