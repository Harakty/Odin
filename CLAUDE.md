# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Odin** - A TCP/UDP network proxy that sits between the Dark Age of Camelot (DAoC) game client (v1.129B) and server. The proxy intercepts and forwards all network packets while extracting game state information for future radar functionality.

**Repository**: https://github.com/Harakty/Odin

## Architecture

The system acts as a transparent man-in-the-middle proxy (single connection):
```
DAoC Client (1.129B) <---> Odin Proxy <---> Game Server
```

## Project Structure

```
Odin.sln
├── src/
│   ├── Odin.Core/           # Core library
│   │   ├── Configuration/   # ProxyConfig
│   │   ├── Network/         # TcpProxy, UdpRelay
│   │   └── Packets/         # PacketReader, DaocPacket
│   └── Odin.App/            # Windows Forms GUI
│       └── MainForm.cs      # Main application window
└── tests/
    └── Odin.Tests/          # Unit tests
```

## Build & Run

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run application
dotnet run --project src/Odin.App/Odin.App.csproj
```

## Key Components

- **TcpProxy**: Listens for client, connects to server, forwards TCP bidirectionally
- **UdpRelay**: Forwards UDP packets between client and server
- **PacketReader**: Parses DAoC packets (TCP: 3-byte header, UDP: 5-byte header, big-endian)
- **MainForm**: GUI with server config, start/stop, log display, packet counter

## Packet Format

TCP: `[Length:2][Code:1][Payload:N]` (big-endian)
UDP: `[Length:2][Counter:2][Code:1][Payload:N]` (big-endian)

## Reference Codebase

DOLSharp server emulator (`D:\Claude-Projects\DOLSharp-2.2.7.3908`) for packet structures.
Key file: `PacketLib1129.cs` (inherits from 1128, no encryption for v1.129B)

## Development Phases

1. **Phase 1 (Complete)**: Transparent proxy with GUI - forwards all packets
2. **Phase 2**: Parse packets by ID, extract position data
3. **Phase 3**: Pass data to external radar DLL
