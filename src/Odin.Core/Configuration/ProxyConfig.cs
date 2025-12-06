namespace Odin.Core.Configuration;

/// <summary>
/// Configuration for the DAoC proxy server.
/// </summary>
public class ProxyConfig
{
    /// <summary>
    /// Local TCP port to listen for client connections.
    /// Default: 10300 (standard DAoC port)
    /// </summary>
    public int LocalTcpPort { get; set; } = 10300;

    /// <summary>
    /// Local UDP port to listen for client packets.
    /// Default: 10300 (same as TCP)
    /// </summary>
    public int LocalUdpPort { get; set; } = 10300;

    /// <summary>
    /// Remote server address (hostname or IP).
    /// </summary>
    public string RemoteServerAddress { get; set; } = string.Empty;

    /// <summary>
    /// Remote server TCP port.
    /// Default: 10300
    /// </summary>
    public int RemoteServerPort { get; set; } = 10300;

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public bool IsValid()
    {
        return LocalTcpPort > 0 && LocalTcpPort <= 65535
            && LocalUdpPort > 0 && LocalUdpPort <= 65535
            && !string.IsNullOrWhiteSpace(RemoteServerAddress)
            && RemoteServerPort > 0 && RemoteServerPort <= 65535;
    }

    /// <summary>
    /// Gets validation error message if configuration is invalid.
    /// </summary>
    public string? GetValidationError()
    {
        if (LocalTcpPort <= 0 || LocalTcpPort > 65535)
        {
            return "Local TCP port must be between 1 and 65535";
        }
        if (LocalUdpPort <= 0 || LocalUdpPort > 65535)
        {
            return "Local UDP port must be between 1 and 65535";
        }
        if (string.IsNullOrWhiteSpace(RemoteServerAddress))
        {
            return "Remote server address is required";
        }
        if (RemoteServerPort <= 0 || RemoteServerPort > 65535)
        {
            return "Remote server port must be between 1 and 65535";
        }
        return null;
    }
}
