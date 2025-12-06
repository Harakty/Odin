namespace Odin.Core.Network;

/// <summary>
/// State of the TCP proxy.
/// </summary>
public enum ProxyState
{
    /// <summary>
    /// Proxy is stopped.
    /// </summary>
    Stopped,

    /// <summary>
    /// Proxy is listening, waiting for client connection.
    /// </summary>
    WaitingForClient,

    /// <summary>
    /// Client connected, proxy is active.
    /// </summary>
    Connected,

    /// <summary>
    /// Connection was lost or closed.
    /// </summary>
    Disconnected
}
