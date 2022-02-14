namespace Horse.WebSocket.Protocol;

/// <summary>
/// WebSocket Protocol OP Codes
/// </summary>
public enum SocketOpCode : byte
{
    /// <summary>
    /// When the data consists of multiple packages, until the last package, all packages must have continue definition
    /// </summary>
    Continue = 0x00,

    /// <summary>
    /// Sending text data definition
    /// </summary>
    UTF8 = 0x01,

    /// <summary>
    /// Sending binary data definition
    /// </summary>
    Binary = 0x02,

    /// <summary>
    /// Terminating connection definition
    /// </summary>
    Terminate = 0x08,

    /// <summary>
    /// Sending ping request definition
    /// </summary>
    Ping = 0x09,

    /// <summary>
    /// Sending response of the ping definition
    /// </summary>
    Pong = 0x0A
}