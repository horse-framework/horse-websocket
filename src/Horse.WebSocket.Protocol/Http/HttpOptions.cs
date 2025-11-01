using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Horse.WebSocket.Protocol.Http;

/// <summary>
/// HTTP Protocol option for Horse HTTP Server
/// </summary>
public class HttpOptions
{
    /// <summary>
    /// Maximum keeping alive duration for each TCP connection
    /// </summary>
    public int HttpConnectionTimeMax { get; set; } = 0;

    /// <summary>
    /// Maximum request lengths (includes content)
    /// </summary>
    public int MaximumRequestLength { get; set; } = 1024 * 100;

    /// <summary>
    /// Supported encodings (Only used when clients accept)
    /// </summary>
    public ContentEncodings[] SupportedEncodings { get; set; }

    /// <summary>
    /// Listening hostnames.
    /// In order to accept all hostnames skip null or set 1-length array with "*" element 
    /// </summary>
    public string[] Hostnames { get; set; }

    /// <summary>
    /// If true, status code responses will have default horse status code response body. Default is true.
    /// </summary>
    public bool UseDefaultStatusCodeResponse { get; set; } = true;

    /// <summary>
    /// Createsd default HTTP server options
    /// </summary>
    public static HttpOptions CreateDefault()
    {
        return new HttpOptions
        {
            HttpConnectionTimeMax = 300,
            MaximumRequestLength = 1024 * 100
        };
    }

    /// <summary>
    /// Loads options from filename
    /// </summary>
    public static HttpOptions Load(string filename)
    {
        string json = File.ReadAllText(filename);

        var stjOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            Converters = { new JsonStringEnumConverter<ContentEncodings>(JsonNamingPolicy.KebabCaseLower, false) }
        };

        var dto = JsonSerializer.Deserialize<HttpOptions>(json, stjOptions);

        HttpOptions options = CreateDefault();

        if (dto is null)
            return options;

        if (dto.HttpConnectionTimeMax > 0)
            options.HttpConnectionTimeMax = dto.HttpConnectionTimeMax;

        if (dto.MaximumRequestLength > 0)
            options.MaximumRequestLength = dto.MaximumRequestLength;

        if (dto.Hostnames is { Length: > 0 })
            options.Hostnames = dto.Hostnames;

        if (dto.SupportedEncodings is { Length: > 0 })
            options.SupportedEncodings = dto.SupportedEncodings;

        return options;
    }
}