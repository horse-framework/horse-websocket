using System;
using System.Threading;
using System.Threading.Tasks;
using Horse.Server;
using Microsoft.Extensions.Hosting;

namespace Horse.WebSocket.Server;

internal class WebSocketRunnerService : IHostedService
{
    private readonly HorseServer _server;
    private readonly IServiceProvider _provider;
    private readonly int _port;

    public WebSocketRunnerService(HorseServer server, IServiceProvider provider, int port)
    {
        _server = server;
        _provider = provider;
        _port = port;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server.Start(_port);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}