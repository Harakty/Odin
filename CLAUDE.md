# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**DoL Radar (DoLDDR)** - A TCP/UDP network proxy that sits between the Dark Age of Camelot (DAoC) game client and server. The proxy intercepts and forwards all network packets while extracting game state information to display a radar overlay showing game objects (players, mobs, NPCs, items, etc.).

## Architecture

The system acts as a transparent man-in-the-middle proxy:
```
DAoC Client <---> DoLDDR Proxy <---> Game Server
```

Key components:
1. **Connection Listener** - Accepts incoming client connections
2. **Server Connector** - Opens connection to the real game server
3. **Packet Forwarder** - Bidirectional TCP/UDP packet relay
4. **Packet Handler** - `ManagePacket()` function for processing packets (initially a stub)
5. **Future: Radar DLL** - External DLL for rendering the radar display

## Reference Codebase

DOLSharp server emulator (`D:\Claude-Projects\DOLSharp-2.2.7.3908`) contains the packet structures and network protocol implementation. Use this to understand:
- How connections are established
- Packet formats and IDs
- Encryption/decryption logic

## Build & Run

This is a C# project targeting Windows (EXE or WinEXE).

```bash
# Build (once project structure is created)
dotnet build

# Run
dotnet run
```

## Development Phases

1. **Phase 1 (Current)**: Transparent proxy - forward all packets, `ManagePacket()` does nothing
2. **Phase 2**: Packet decryption and ID filtering
3. **Phase 3**: Pass decoded data to external radar DLL
