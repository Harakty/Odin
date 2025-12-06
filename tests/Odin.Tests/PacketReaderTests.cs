using Odin.Core.Packets;

namespace Odin.Tests;

public class PacketReaderTests
{
    private readonly PacketReader _reader = new();

    [Fact]
    public void TryReadTcpPacket_WithCompletePacket_ReturnsTrue()
    {
        // Arrange: Create a TCP packet with length=5 (code + 4 bytes payload)
        // Total: 2 (length) + 5 (length value) = 7 bytes
        byte[] buffer =
        [
            0x00, 0x05,       // Length: 5 (big-endian)
            0xA9,             // Packet code
            0x01, 0x02, 0x03, 0x04  // Payload
        ];

        // Act
        bool result = _reader.TryReadTcpPacket(
            buffer, 0, buffer.Length,
            PacketDirection.ClientToServer,
            out DaocPacket packet,
            out int bytesConsumed);

        // Assert
        Assert.True(result);
        Assert.Equal(5, packet.Length);
        Assert.Equal(0xA9, packet.PacketCode);
        Assert.Equal(7, bytesConsumed);
        Assert.Equal(4, packet.Payload.Length);
        Assert.False(packet.IsUdp);
        Assert.Equal(PacketDirection.ClientToServer, packet.Direction);
    }

    [Fact]
    public void TryReadTcpPacket_WithIncompletePacket_ReturnsFalse()
    {
        // Arrange: Incomplete packet (length says 10, but only 5 bytes available)
        byte[] buffer =
        [
            0x00, 0x0A,       // Length: 10 (big-endian)
            0xA9,             // Packet code
            0x01, 0x02        // Only 2 bytes of payload (need 9)
        ];

        // Act
        bool result = _reader.TryReadTcpPacket(
            buffer, 0, buffer.Length,
            PacketDirection.ClientToServer,
            out _,
            out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryReadTcpPacket_WithInsufficientHeader_ReturnsFalse()
    {
        // Arrange: Less than 3 bytes (minimum TCP header)
        byte[] buffer = [0x00, 0x05];

        // Act
        bool result = _reader.TryReadTcpPacket(
            buffer, 0, buffer.Length,
            PacketDirection.ClientToServer,
            out _,
            out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryReadUdpPacket_WithCompletePacket_ReturnsTrue()
    {
        // Arrange: Create a UDP packet with length=7 (counter + code + 4 bytes payload)
        // Total: 2 (length) + 7 (length value) = 9 bytes
        byte[] buffer =
        [
            0x00, 0x07,       // Length: 7 (big-endian)
            0x00, 0x42,       // UDP Counter: 66 (big-endian)
            0xA9,             // Packet code
            0x01, 0x02, 0x03, 0x04  // Payload
        ];

        // Act
        bool result = _reader.TryReadUdpPacket(
            buffer, 0, buffer.Length,
            PacketDirection.ServerToClient,
            out DaocPacket packet,
            out int bytesConsumed);

        // Assert
        Assert.True(result);
        Assert.Equal(7, packet.Length);
        Assert.Equal(0xA9, packet.PacketCode);
        Assert.Equal(66, packet.UdpCounter);
        Assert.Equal(9, bytesConsumed);
        Assert.Equal(4, packet.Payload.Length);
        Assert.True(packet.IsUdp);
        Assert.Equal(PacketDirection.ServerToClient, packet.Direction);
    }

    [Fact]
    public void TryReadUdpPacket_WithIncompletePacket_ReturnsFalse()
    {
        // Arrange: Incomplete UDP packet
        byte[] buffer =
        [
            0x00, 0x0A,       // Length: 10 (big-endian)
            0x00, 0x42,       // UDP Counter
            0xA9              // Packet code (missing payload)
        ];

        // Act
        bool result = _reader.TryReadUdpPacket(
            buffer, 0, buffer.Length,
            PacketDirection.ServerToClient,
            out _,
            out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryReadTcpPacket_WithOffset_ReadsFromCorrectPosition()
    {
        // Arrange: Buffer with garbage before the packet
        byte[] buffer =
        [
            0xFF, 0xFF, 0xFF, // Garbage
            0x00, 0x03,       // Length: 3 (big-endian)
            0xD4,             // Packet code (PlayerCreate)
            0xAA, 0xBB        // Payload
        ];

        // Act
        bool result = _reader.TryReadTcpPacket(
            buffer, 3, buffer.Length - 3,
            PacketDirection.ServerToClient,
            out DaocPacket packet,
            out int bytesConsumed);

        // Assert
        Assert.True(result);
        Assert.Equal(3, packet.Length);
        Assert.Equal(0xD4, packet.PacketCode);
        Assert.Equal(5, bytesConsumed);
    }

    [Fact]
    public void ReadUInt16BigEndian_ReturnsCorrectValue()
    {
        // Arrange
        byte[] data = [0x12, 0x34];

        // Act
        ushort result = PacketReader.ReadUInt16BigEndian(data);

        // Assert
        Assert.Equal(0x1234, result);
    }

    [Fact]
    public void ReadUInt32BigEndian_ReturnsCorrectValue()
    {
        // Arrange
        byte[] data = [0x12, 0x34, 0x56, 0x78];

        // Act
        uint result = PacketReader.ReadUInt32BigEndian(data);

        // Assert
        Assert.Equal(0x12345678u, result);
    }
}
