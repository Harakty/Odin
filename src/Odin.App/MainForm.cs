using Odin.Core.Configuration;
using Odin.Core.Network;
using Odin.Core.Packets;
using Odin.Core.Services;

namespace Odin.App;

public partial class MainForm : Form
{
    private TcpProxy? _tcpProxy;
    private UdpRelay? _udpRelay;
    private readonly FileLockService _fileLockService;
    private readonly AppSettings _settings;
    private int _packetCount;
    private bool _isRunning;

    public MainForm()
    {
        InitializeComponent();

        _fileLockService = new FileLockService();
        _settings = AppSettings.Load();

        // Wire up events
        btnStartStop.Click += BtnStartStop_Click;
        btnBrowse.Click += BtnBrowse_Click;
        btnLock.Click += BtnLock_Click;
        this.FormClosing += MainForm_FormClosing;
        this.Load += MainForm_Load;

        // Wire up FileLockService events
        _fileLockService.OnLockStateChanged += locked => UpdateLockUI(locked);
        _fileLockService.OnError += error => MessageBox.Show(error, "File Lock Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        // Load saved settings into UI
        txtServerAddress.Text = _settings.RemoteServerAddress;
        nudServerPort.Value = Math.Clamp(_settings.RemoteServerPort, 1, 65535);
        nudLocalPort.Value = Math.Clamp(_settings.LocalPort, 1, 65535);
        txtFilePath.Text = _settings.LockedFilePath;

        // Enable lock button if file path is set
        btnLock.Enabled = !string.IsNullOrWhiteSpace(_settings.LockedFilePath);
    }

    private void SaveSettings()
    {
        _settings.RemoteServerAddress = txtServerAddress.Text.Trim();
        _settings.RemoteServerPort = (int)nudServerPort.Value;
        _settings.LocalPort = (int)nudLocalPort.Value;
        _settings.LockedFilePath = txtFilePath.Text;
        _settings.Save();
    }

    private void BtnBrowse_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select file to lock",
            Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
            CheckFileExists = true
        };

        if (!string.IsNullOrWhiteSpace(txtFilePath.Text) && File.Exists(txtFilePath.Text))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(txtFilePath.Text);
            dialog.FileName = Path.GetFileName(txtFilePath.Text);
        }

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            txtFilePath.Text = dialog.FileName;
            btnLock.Enabled = true;
            SaveSettings();
        }
    }

    private void BtnLock_Click(object? sender, EventArgs e)
    {
        if (_fileLockService.IsLocked)
        {
            _fileLockService.UnlockFile();
            AppendLog($"File unlocked: {txtFilePath.Text}");
        }
        else
        {
            if (_fileLockService.LockFile(txtFilePath.Text))
            {
                AppendLog($"File locked: {txtFilePath.Text}");
            }
        }
    }

    private void UpdateLockUI(bool locked)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => UpdateLockUIInternal(locked));
        }
        else
        {
            UpdateLockUIInternal(locked);
        }
    }

    private void UpdateLockUIInternal(bool locked)
    {
        if (locked)
        {
            btnLock.Text = "Unlock";
            lblLockStatus.Text = "LOCKED";
            lblLockStatus.BackColor = Color.LightGreen;
            lblLockStatus.ForeColor = Color.DarkGreen;
            btnBrowse.Enabled = false;
        }
        else
        {
            btnLock.Text = "Lock";
            lblLockStatus.Text = "";
            lblLockStatus.BackColor = Color.Transparent;
            btnBrowse.Enabled = true;
        }
    }

    private async void BtnStartStop_Click(object? sender, EventArgs e)
    {
        if (_isRunning)
        {
            StopProxy();
        }
        else
        {
            SaveSettings();
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

        // Create configuration from settings
        var config = _settings.ToProxyConfig();

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
        // Check if file is locked
        if (_fileLockService.IsLocked)
        {
            var result = MessageBox.Show(
                "A file is currently locked. Do you want to unlock it and close?",
                "File Locked",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            _fileLockService.UnlockFile();
        }

        SaveSettings();
        StopProxy();
        _fileLockService.Dispose();
    }
}
