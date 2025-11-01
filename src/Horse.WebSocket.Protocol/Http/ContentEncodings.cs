namespace Horse.WebSocket.Protocol.Http;

/// Content encodings
/// </summary>
public enum ContentEncodings
{
    /// <summary>
    /// Plain text, no encoding is applied
    /// </summary>
    None,

    /// <summary>
    /// GZIP Encoding
    /// </summary>
    Gzip,

    /// <summary>
    /// Brotli encoding, a.k.a. br
    /// </summary>
    Brotli,

    /// <summary>
    /// Deflate encoding
    /// </summary>
    Deflate
}