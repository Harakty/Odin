namespace Odin.Core.Packets;

/// <summary>
/// Represents a parsed DAoC packet.
/// </summary>
public readonly struct DaocPacket
{
    /// <summary>
    /// Packet length (from header, big-endian).
    /// </summary>
    public ushort Length { get; init; }

    /// <summary>
    /// Packet code/ID.
    /// </summary>
    public byte PacketCode { get; init; }

    /// <summary>
    /// UDP counter for sequencing (UDP only, 0 for TCP).
    /// </summary>
    public ushort UdpCounter { get; init; }

    /// <summary>
    /// Raw packet data including header.
    /// </summary>
    public ReadOnlyMemory<byte> RawData { get; init; }

    /// <summary>
    /// Payload data (after header).
    /// </summary>
    public ReadOnlyMemory<byte> Payload { get; init; }

    /// <summary>
    /// True if this is a UDP packet.
    /// </summary>
    public bool IsUdp { get; init; }

    /// <summary>
    /// Direction of the packet.
    /// </summary>
    public PacketDirection Direction { get; init; }

    /// <summary>
    /// Total size of the packet including header.
    /// </summary>
    public int TotalSize => RawData.Length;
}
