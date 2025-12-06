namespace Odin.Core.Packets;

/// <summary>
/// Direction of packet flow.
/// </summary>
public enum PacketDirection
{
    /// <summary>
    /// Packet from client to server.
    /// </summary>
    ClientToServer,

    /// <summary>
    /// Packet from server to client.
    /// </summary>
    ServerToClient
}
