using Odin.Core.Configuration;
using Odin.Core.Network;
using Odin.Core.Packets;

namespace Odin.App;

public partial class MainForm : Form
{
    private TcpProxy? _tcpProxy;
    private UdpRelay? _udpRelay;
    private int _packetCount;
    private bool _isRunning;

    public MainForm()
    {
        InitializeComponent();
        btnStartStop.Click += BtnStartStop_Click;
        this.FormClosing += MainForm_FormClosing;
    }

    private async void BtnStartStop_Click(object? sender, EventArgs e)
    {
        if (_isRunning)
        {
            StopProxy();
        }
        else
        {
            await StartProxyAsync();
        }
    }

    private async Task StartProxyAsync()
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(txtServerAddress.Text))
        {
            MessageBox.Show("Please enter a server address.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Create configuration
        var config = new ProxyConfig
        {
            LocalTcpPort = (int)nudLocalPort.Value,
            LocalUdpPort = (int)nudLocalPort.Value,
            RemoteServerAddress = txtServerAddress.Text.Trim(),
            RemoteServerPort = (int)nudServerPort.Value
        };

        // Validate configuration
        var validationError = config.GetValidationError();
        if (validationError != null)
        {
            MessageBox.Show(validationError, "Configuration Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Reset state
        _packetCount = 0;
        UpdatePacketCount();

        // Create and configure TCP proxy
        _tcpProxy = new TcpProxy(config);
        _tcpProxy.OnLog += message => AppendLog(message);
        _tcpProxy.OnPacketReceived += HandlePacket;
        _tcpProxy.OnClientConnected += () => UpdateStatus("Client connected");
        _tcpProxy.OnServerConnected += () => UpdateStatus("Connected to server");
        _tcpProxy.OnDisconnected += reason =>
        {
            UpdateStatus($"Disconnected: {reason}");
            BeginInvoke(() => OnProxyStopped());
        };

        // Create and configure UDP relay
        _udpRelay = new UdpRelay(config);
        _udpRelay.OnLog += message => AppendLog(message);
        _udpRelay.OnPacketReceived += HandlePacket;

        // Update UI
        _isRunning = true;
        btnStartStop.Text = "Stop";
        SetConfigEnabled(false);
        UpdateStatus("Starting...");

        try
        {
            // Start UDP relay
            _udpRelay.Start();

            // Start TCP proxy (this will block until connection closes)
            await _tcpProxy.StartAsync();
        }
        catch (Exception ex)
        {
            AppendLog($"Error: {ex.Message}");
            MessageBox.Show($"Failed to start proxy: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            OnProxyStopped();
        }
    }

    private void StopProxy()
    {
        _tcpProxy?.Stop();
        _udpRelay?.Stop();
    }

    private void OnProxyStopped()
    {
        _tcpProxy?.Dispose();
        _udpRelay?.Dispose();
        _tcpProxy = null;
        _udpRelay = null;

        _isRunning = false;
        btnStartStop.Text = "Start";
        SetConfigEnabled(true);
        UpdateStatus("Stopped");
    }

    private void HandlePacket(DaocPacket packet)
    {
        // ManagePacket hook - currently just counts packets
        _packetCount++;

        // Update UI (thread-safe)
        if (InvokeRequired)
        {
            BeginInvoke(UpdatePacketCount);
        }
        else
        {
            UpdatePacketCount();
        }
    }

    private void SetConfigEnabled(bool enabled)
    {
        txtServerAddress.Enabled = enabled;
        nudServerPort.Enabled = enabled;
        nudLocalPort.Enabled = enabled;
    }

    private void UpdateStatus(string status)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => lblStatus.Text = $"Status: {status}");
        }
        else
        {
            lblStatus.Text = $"Status: {status}";
        }
    }

    private void UpdatePacketCount()
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => lblPacketCount.Text = $"Packets: {_packetCount}");
        }
        else
        {
            lblPacketCount.Text = $"Packets: {_packetCount}";
        }
    }

    private void AppendLog(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => AppendLogInternal(message));
        }
        else
        {
            AppendLogInternal(message);
        }
    }

    private void AppendLogInternal(string message)
    {
        txtLog.AppendText(message + Environment.NewLine);
        txtLog.ScrollToCaret();

        // Limit log size
        if (txtLog.TextLength > 100000)
        {
            txtLog.Text = txtLog.Text.Substring(txtLog.TextLength - 50000);
        }
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        StopProxy();
    }
}
