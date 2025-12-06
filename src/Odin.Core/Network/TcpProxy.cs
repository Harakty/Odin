using System.Net;
using System.Net.Sockets;
using Odin.Core.Configuration;
using Odin.Core.Packets;

namespace Odin.Core.Network;

/// <summary>
/// TCP proxy for a single DAoC client-server connection.
/// Listens for a client connection, connects to the server, and forwards data bidirectionally.
/// </summary>
public class TcpProxy : IDisposable
{
    private const int BufferSize = 8192;

    private readonly ProxyConfig _config;
    private readonly PacketReader _packetReader;

    private TcpListener? _listener;
    private TcpClient? _clientConnection;
    private TcpClient? _serverConnection;
    private NetworkStream? _clientStream;
    private NetworkStream? _serverStream;

    private CancellationTokenSource? _cts;
    private bool _disposed;

    /// <summary>
    /// Current state of the proxy.
    /// </summary>
    public ProxyState State { get; private set; } = ProxyState.Stopped;

    /// <summary>
    /// Event raised when a packet is received (for ManagePacket hook).
    /// </summary>
    public event Action<DaocPacket>? OnPacketReceived;

    /// <summary>
    /// Event raised when a log message is generated.
    /// </summary>
    public event Action<string>? OnLog;

    /// <summary>
    /// Event raised when client connects to proxy.
    /// </summary>
    public event Action? OnClientConnected;

    /// <summary>
    /// Event raised when connection is established to server.
    /// </summary>
    public event Action? OnServerConnected;

    /// <summary>
    /// Event raised when disconnection occurs.
    /// </summary>
    public event Action<string>? OnDisconnected;

    /// <summary>
    /// Creates a new TCP proxy with the specified configuration.
    /// </summary>
    public TcpProxy(ProxyConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _packetReader = new PacketReader();
    }

    /// <summary>
    /// Starts the proxy listener.
    /// </summary>
    public async Task StartAsync()
    {
        if (State != ProxyState.Stopped)
        {
            throw new InvalidOperationException("Proxy is already running");
        }

        var validationError = _config.GetValidationError();
        if (validationError != null)
        {
            throw new InvalidOperationException($"Invalid configuration: {validationError}");
        }

        _cts = new CancellationTokenSource();

        try
        {
            _listener = new TcpListener(IPAddress.Any, _config.LocalTcpPort);
            _listener.Start();

            State = ProxyState.WaitingForClient;
            Log($"Listening on port {_config.LocalTcpPort}...");

            // Wait for client connection
            _clientConnection = await _listener.AcceptTcpClientAsync(_cts.Token);
            _clientStream = _clientConnection.GetStream();

            var clientEndpoint = _clientConnection.Client.RemoteEndPoint as IPEndPoint;
            Log($"Client connected from {clientEndpoint?.Address}:{clientEndpoint?.Port}");
            OnClientConnected?.Invoke();

            // Stop listening (single connection only)
            _listener.Stop();

            // Connect to server
            Log($"Connecting to server {_config.RemoteServerAddress}:{_config.RemoteServerPort}...");
            _serverConnection = new TcpClient();
            await _serverConnection.ConnectAsync(
                _config.RemoteServerAddress,
                _config.RemoteServerPort,
                _cts.Token);
            _serverStream = _serverConnection.GetStream();

            State = ProxyState.Connected;
            Log("Connected to server");
            OnServerConnected?.Invoke();

            // Start bidirectional forwarding
            var clientToServerTask = ForwardDataAsync(
                _clientStream, _serverStream,
                PacketDirection.ClientToServer,
                "Client", "Server",
                _cts.Token);

            var serverToClientTask = ForwardDataAsync(
                _serverStream, _clientStream,
                PacketDirection.ServerToClient,
                "Server", "Client",
                _cts.Token);

            // Wait for either direction to complete (disconnection)
            await Task.WhenAny(clientToServerTask, serverToClientTask);

            // Cancel the other direction
            _cts.Cancel();
        }
        catch (OperationCanceledException)
        {
            Log("Proxy stopped by user");
        }
        catch (Exception ex)
        {
            Log($"Error: {ex.Message}");
        }
        finally
        {
            Cleanup();
            State = ProxyState.Disconnected;
            OnDisconnected?.Invoke("Connection closed");
        }
    }

    /// <summary>
    /// Stops the proxy.
    /// </summary>
    public void Stop()
    {
        Log("Stopping proxy...");
        _cts?.Cancel();
    }

    /// <summary>
    /// Forwards data from source to destination stream, parsing packets.
    /// </summary>
    private async Task ForwardDataAsync(
        NetworkStream source,
        NetworkStream destination,
        PacketDirection direction,
        string sourceName,
        string destName,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[BufferSize];
        var packetBuffer = new byte[BufferSize * 2];
        int packetBufferOffset = 0;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                int bytesRead = await source.ReadAsync(
                    buffer.AsMemory(0, buffer.Length),
                    cancellationToken);

                if (bytesRead == 0)
                {
                    Log($"{sourceName} disconnected");
                    break;
                }

                // Copy to packet buffer for parsing
                if (packetBufferOffset + bytesRead <= packetBuffer.Length)
                {
                    Buffer.BlockCopy(buffer, 0, packetBuffer, packetBufferOffset, bytesRead);
                    packetBufferOffset += bytesRead;
                }
                else
                {
                    // Buffer overflow - reset and continue
                    Log($"Warning: Packet buffer overflow, resetting");
                    packetBufferOffset = 0;
                    Buffer.BlockCopy(buffer, 0, packetBuffer, 0, bytesRead);
                    packetBufferOffset = bytesRead;
                }

                // Try to parse packets from buffer
                int offset = 0;
                while (offset < packetBufferOffset)
                {
                    if (_packetReader.TryReadTcpPacket(
                        packetBuffer, offset, packetBufferOffset - offset,
                        direction,
                        out DaocPacket packet,
                        out int consumed))
                    {
                        // Call ManagePacket hook
                        try
                        {
                            OnPacketReceived?.Invoke(packet);
                        }
                        catch (Exception ex)
                        {
                            Log($"Error in packet handler: {ex.Message}");
                        }

                        offset += consumed;
                    }
                    else
                    {
                        // Incomplete packet, wait for more data
                        break;
                    }
                }

                // Compact buffer - remove processed data
                if (offset > 0 && offset < packetBufferOffset)
                {
                    Buffer.BlockCopy(packetBuffer, offset, packetBuffer, 0, packetBufferOffset - offset);
                    packetBufferOffset -= offset;
                }
                else if (offset >= packetBufferOffset)
                {
                    packetBufferOffset = 0;
                }

                // Forward raw data to destination (transparent proxy)
                await destination.WriteAsync(
                    buffer.AsMemory(0, bytesRead),
                    cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
        catch (IOException ex)
        {
            Log($"{sourceName} connection error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Log($"Forward error ({sourceName}->{destName}): {ex.Message}");
        }
    }

    /// <summary>
    /// Cleans up resources.
    /// </summary>
    private void Cleanup()
    {
        _listener?.Stop();
        _clientStream?.Dispose();
        _serverStream?.Dispose();
        _clientConnection?.Dispose();
        _serverConnection?.Dispose();

        _listener = null;
        _clientStream = null;
        _serverStream = null;
        _clientConnection = null;
        _serverConnection = null;
    }

    /// <summary>
    /// Logs a message.
    /// </summary>
    private void Log(string message)
    {
        OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
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
