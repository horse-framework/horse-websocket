using Horse.Core;

namespace Horse.WebSocket.Protocol.Http;

/// <summary>
/// Extention methods for Horse HTTP protocol
/// </summary>
public static class HorseHttpExtensions
{
    /// <summary>
    /// Uses HTTP Protocol and accepts HTTP connections
    /// </summary>
    public static IHorseServer UseHttp(this IHorseServer server, HttpRequestHandler action)
    {
        return UseHttp(server, action, HttpOptions.CreateDefault());
    }

    /// <summary>
    /// Uses HTTP Protocol and accepts HTTP connections
    /// </summary>
    public static IHorseServer UseHttp(this IHorseServer server, HttpRequestHandler action, HttpOptions options)
    {
        HttpMethodHandler handler = new HttpMethodHandler(action);
        HorseHttpProtocol protocol = new HorseHttpProtocol(server, handler, options);
        server.UseProtocol(protocol);
        return server;
    }

    /// <summary>
    /// Uses HTTP Protocol and accepts HTTP connections
    /// </summary>
    public static IHorseServer UseHttp(this IHorseServer server, HttpRequestHandler action, string optionsFilename)
    {
        HttpMethodHandler handler = new HttpMethodHandler(action);
        HorseHttpProtocol protocol = new HorseHttpProtocol(server, handler, HttpOptions.Load(optionsFilename));
        server.UseProtocol(protocol);
        return server;
    }
}