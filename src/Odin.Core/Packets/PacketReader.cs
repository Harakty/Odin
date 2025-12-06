using System.Buffers.Binary;

namespace Odin.Core.Packets;

/// <summary>
/// Reads and parses DAoC packets from byte buffers.
/// Handles packet fragmentation and big-endian format.
/// </summary>
public class PacketReader
{
    /// <summary>
    /// TCP header size: 2 bytes length + 1 byte code.
    /// </summary>
    public const int TcpHeaderSize = 3;

    /// <summary>
    /// UDP header size: 2 bytes length + 2 bytes counter + 1 byte code.
    /// </summary>
    public const int UdpHeaderSize = 5;

    /// <summary>
    /// Minimum TCP packet size (header only, no payload).
    /// </summary>
    public const int MinTcpPacketSize = TcpHeaderSize;

    /// <summary>
    /// Minimum UDP packet size (header only, no payload).
    /// </summary>
    public const int MinUdpPacketSize = UdpHeaderSize;

    /// <summary>
    /// Tries to read a TCP packet from the buffer.
    /// </summary>
    /// <param name="buffer">Buffer containing packet data.</param>
    /// <param name="offset">Offset in buffer to start reading.</param>
    /// <param name="length">Available bytes in buffer from offset.</param>
    /// <param name="direction">Direction of the packet.</param>
    /// <param name="packet">Parsed packet if successful.</param>
    /// <param name="bytesConsumed">Number of bytes consumed from buffer.</param>
    /// <returns>True if a complete packet was read.</returns>
    public bool TryReadTcpPacket(
        byte[] buffer,
        int offset,
        int length,
        PacketDirection direction,
        out DaocPacket packet,
        out int bytesConsumed)
    {
        packet = default;
        bytesConsumed = 0;

        // Need at least header to read length
        if (length < TcpHeaderSize)
        {
            return false;
        }

        // Read length (big-endian)
        ushort packetLength = BinaryPrimitives.ReadUInt16BigEndian(
            buffer.AsSpan(offset, 2));

        // Total packet size = length field value + 2 (length field itself) + 1 (code)
        // In DAoC, the length field typically includes the code byte
        // Total = 2 (length) + packetLength
        int totalSize = 2 + packetLength;

        // Check if we have the complete packet
        if (length < totalSize)
        {
            return false;
        }

        // Read packet code
        byte packetCode = buffer[offset + 2];

        // Create packet structure
        var rawData = new byte[totalSize];
        Buffer.BlockCopy(buffer, offset, rawData, 0, totalSize);

        int payloadOffset = TcpHeaderSize;
        int payloadLength = totalSize - TcpHeaderSize;

        packet = new DaocPacket
        {
            Length = packetLength,
            PacketCode = packetCode,
            UdpCounter = 0,
            RawData = rawData,
            Payload = payloadLength > 0
                ? new ReadOnlyMemory<byte>(rawData, payloadOffset, payloadLength)
                : ReadOnlyMemory<byte>.Empty,
            IsUdp = false,
            Direction = direction
        };

        bytesConsumed = totalSize;
        return true;
    }

    /// <summary>
    /// Tries to read a UDP packet from the buffer.
    /// </summary>
    /// <param name="buffer">Buffer containing packet data.</param>
    /// <param name="offset">Offset in buffer to start reading.</param>
    /// <param name="length">Available bytes in buffer from offset.</param>
    /// <param name="direction">Direction of the packet.</param>
    /// <param name="packet">Parsed packet if successful.</param>
    /// <param name="bytesConsumed">Number of bytes consumed from buffer.</param>
    /// <returns>True if a complete packet was read.</returns>
    public bool TryReadUdpPacket(
        byte[] buffer,
        int offset,
        int length,
        PacketDirection direction,
        out DaocPacket packet,
        out int bytesConsumed)
    {
        packet = default;
        bytesConsumed = 0;

        // Need at least header to read length
        if (length < UdpHeaderSize)
        {
            return false;
        }

        // Read length (big-endian)
        ushort packetLength = BinaryPrimitives.ReadUInt16BigEndian(
            buffer.AsSpan(offset, 2));

        // Read UDP counter (big-endian)
        ushort udpCounter = BinaryPrimitives.ReadUInt16BigEndian(
            buffer.AsSpan(offset + 2, 2));

        // Total packet size = 2 (length) + packetLength
        int totalSize = 2 + packetLength;

        // Check if we have the complete packet
        if (length < totalSize)
        {
            return false;
        }

        // Read packet code
        byte packetCode = buffer[offset + 4];

        // Create packet structure
        var rawData = new byte[totalSize];
        Buffer.BlockCopy(buffer, offset, rawData, 0, totalSize);

        int payloadOffset = UdpHeaderSize;
        int payloadLength = totalSize - UdpHeaderSize;

        packet = new DaocPacket
        {
            Length = packetLength,
            PacketCode = packetCode,
            UdpCounter = udpCounter,
            RawData = rawData,
            Payload = payloadLength > 0
                ? new ReadOnlyMemory<byte>(rawData, payloadOffset, payloadLength)
                : ReadOnlyMemory<byte>.Empty,
            IsUdp = true,
            Direction = direction
        };

        bytesConsumed = totalSize;
        return true;
    }

    /// <summary>
    /// Reads a 16-bit unsigned integer in big-endian format.
    /// </summary>
    public static ushort ReadUInt16BigEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadUInt16BigEndian(source);
    }

    /// <summary>
    /// Reads a 32-bit unsigned integer in big-endian format.
    /// </summary>
    public static uint ReadUInt32BigEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadUInt32BigEndian(source);
    }

    /// <summary>
    /// Reads a 16-bit signed integer in big-endian format.
    /// </summary>
    public static short ReadInt16BigEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadInt16BigEndian(source);
    }

    /// <summary>
    /// Reads a 32-bit signed integer in big-endian format.
    /// </summary>
    public static int ReadInt32BigEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadInt32BigEndian(source);
    }
}
