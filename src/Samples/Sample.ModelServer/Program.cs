﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Horse.Protocols.Http;
using Horse.Server;
using Horse.WebSocket.Protocol;
using Horse.WebSocket.Protocol.Serialization;
using Horse.WebSocket.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.ModelServer.Handlers;
using Sample.ModelServer.Models;

namespace Sample.ModelServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .UseHorseWebSocketServer((context, builder) =>
                {
                    builder.UsePayloadModelProvider(new SystemJsonModelSerializer());
                    builder.AddSingletonHandlers(typeof(Program));
                    builder.OnClientReady((provider, client) =>
                        {
                            Console.WriteLine("Client connected");
                            return Task.CompletedTask;
                        })
                        .OnClientDisconnected((provider, client) =>
                        {
                            Console.WriteLine("Client disconnected");
                            return Task.CompletedTask;
                        })
                        .OnError((exception, message, client) => Console.WriteLine("Error: " + exception));

                    builder.UsePort(888);
                })
                .Build();

            HorseServer server = host.Services.GetService<HorseServer>();
            server.UseHttp((request, response) =>
            {
                if (request.Path.Equals("/status", StringComparison.InvariantCultureIgnoreCase))
                {
                    response.SetToText();
                    response.StatusCode = HttpStatusCode.OK;
                    response.Write("OK");
                }
                else
                    response.StatusCode = HttpStatusCode.NotFound;

                return Task.CompletedTask;
            });

            host.Run();
        }
    }
}