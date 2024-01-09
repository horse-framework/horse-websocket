using System;

namespace Horse.WebSocket.Protocol.Security;

/// <summary>
/// Authenticate attribute for IWebSocketMessageHandler classes.
/// Checks if IsAuthenticated property is true or not.
/// In additionaly checks if you implement IWebSocketAuthenticator implementation
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AuthenticateAttribute : Attribute
{
}