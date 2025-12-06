using System.Net;
using System.Net.Sockets;
using Odin.Core.Configuration;
using Odin.Core.Packets;

namespace Odin.Core.Network;

/// <summary>
/// UDP relay for a single DAoC client-server connection.
/// Receives UDP packets from client, forwards to server, and vice versa.
/// </summary>
public class UdpRelay : IDisposable
{
    private const int BufferSize = 4096;

    private readonly ProxyConfig _config;
    private readonly PacketReader _packetReader;

    private UdpClient? _udpClient;
    private IPEndPoint? _clientEndpoint;
    private IPEndPoint? _serverEndpoint;
    private CancellationTokenSource? _cts;
    private bool _disposed;
    private bool _running;

    /// <summary>
    /// Event raised when a UDP packet is received (for ManagePacket hook).
    /// </summary>
    public event Action<DaocPacket>? OnPacketReceived;

    /// <summary>
    /// Event raised when a log message is generated.
    /// </summary>
    public event Action<string>? OnLog;

    /// <summary>
    /// Creates a new UDP relay with the specified configuration.
    /// </summary>
    public UdpRelay(ProxyConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _packetReader = new PacketReader();
    }

    /// <summary>
    /// Starts the UDP relay.
    /// </summary>
    public void Start()
    {
        if (_running)
        {
            throw new InvalidOperationException("UDP relay is already running");
        }

        _cts = new CancellationTokenSource();

        try
        {
            // Create UDP socket bound to local port
            _udpClient = new UdpClient(_config.LocalUdpPort);

            // Resolve server endpoint
            var serverAddresses = Dns.GetHostAddresses(_config.RemoteServerAddress);
            if (serverAddresses.Length == 0)
            {
                throw new Exception($"Cannot resolve server address: {_config.RemoteServerAddress}");
            }
            _serverEndpoint = new IPEndPoint(serverAddresses[0], _config.RemoteServerPort);

            _running = true;
            Log($"UDP relay started on port {_config.LocalUdpPort}");

            // Start receive loop in background
            _ = ReceiveLoopAsync(_cts.Token);
        }
        catch (Exception ex)
        {
            Log($"Failed to start UDP relay: {ex.Message}");
            Cleanup();
            throw;
        }
    }

    /// <summary>
    /// Stops the UDP relay.
    /// </summary>
    public void Stop()
    {
        if (!_running)
        {
            return;
        }

        Log("Stopping UDP relay...");
        _cts?.Cancel();
        _running = false;
        Cleanup();
    }

    /// <summary>
    /// Main receive loop for UDP packets.
    /// </summary>
    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && _udpClient != null)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync(cancellationToken);
                    var senderEndpoint = result.RemoteEndPoint;
                    var data = result.Buffer;

                    if (data.Length == 0)
                    {
                        continue;
                    }

                    // Determine direction based on sender
                    bool fromServer = _serverEndpoint != null &&
                                      senderEndpoint.Address.Equals(_serverEndpoint.Address) &&
                                      senderEndpoint.Port == _serverEndpoint.Port;

                    if (fromServer)
                    {
                        // Server -> Client
                        await HandleServerToClientAsync(data, cancellationToken);
                    }
                    else
                    {
                        // Client -> Server (first packet sets client endpoint)
                        if (_clientEndpoint == null)
                        {
                            _clientEndpoint = senderEndpoint;
                            Log($"UDP client endpoint set to {_clientEndpoint}");
                        }
                        await HandleClientToServerAsync(data, senderEndpoint, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (SocketException ex)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        Log($"UDP socket error: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                Log($"UDP receive loop error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Handles UDP packet from client to server.
    /// </summary>
    private async Task HandleClientToServerAsync(
        byte[] data,
        IPEndPoint clientEndpoint,
        CancellationToken cancellationToken)
    {
        // Parse packet for ManagePacket hook
        if (_packetReader.TryReadUdpPacket(
            data, 0, data.Length,
            PacketDirection.ClientToServer,
            out DaocPacket packet,
            out _))
        {
            try
            {
                OnPacketReceived?.Invoke(packet);
            }
            catch (Exception ex)
            {
                Log($"Error in UDP packet handler: {ex.Message}");
            }
        }

        // Forward to server
        if (_serverEndpoint != null && _udpClient != null)
        {
            await _udpClient.SendAsync(data, _serverEndpoint, cancellationToken);
        }
    }

    /// <summary>
    /// Handles UDP packet from server to client.
    /// </summary>
    private async Task HandleServerToClientAsync(
        byte[] data,
        CancellationToken cancellationToken)
    {
        // Parse packet for ManagePacket hook
        if (_packetReader.TryReadUdpPacket(
            data, 0, data.Length,
            PacketDirection.ServerToClient,
            out DaocPacket packet,
            out _))
        {
            try
            {
                OnPacketReceived?.Invoke(packet);
            }
            catch (Exception ex)
            {
                Log($"Error in UDP packet handler: {ex.Message}");
            }
        }

        // Forward to client
        if (_clientEndpoint != null && _udpClient != null)
        {
            await _udpClient.SendAsync(data, _clientEndpoint, cancellationToken);
        }
    }

    /// <summary>
    /// Cleans up resources.
    /// </summary>
    private void Cleanup()
    {
        _udpClient?.Dispose();
        _udpClient = null;
        _clientEndpoint = null;
    }

    /// <summary>
    /// Logs a message.
    /// </summary>
    private void Log(string message)
    {
        OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] [UDP] {message}");
    }

    /// <summary>
    /// Disposes resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _cts?.Cancel();
        _cts?.Dispose();
        Cleanup();

        GC.SuppressFinalize(this);
    }
}
