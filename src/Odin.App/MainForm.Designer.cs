namespace Odin.App;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 620);
        this.Text = "Odin - DAoC Proxy";
        this.MinimumSize = new System.Drawing.Size(600, 500);

        // GroupBox for Proxy Configuration
        grpConfig = new GroupBox();
        grpConfig.Text = "Proxy Configuration";
        grpConfig.Location = new Point(12, 12);
        grpConfig.Size = new Size(776, 100);
        grpConfig.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        // Server Address
        lblServerAddress = new Label();
        lblServerAddress.Text = "Server Address:";
        lblServerAddress.Location = new Point(10, 25);
        lblServerAddress.AutoSize = true;

        txtServerAddress = new TextBox();
        txtServerAddress.Location = new Point(110, 22);
        txtServerAddress.Size = new Size(200, 23);

        // Server Port
        lblServerPort = new Label();
        lblServerPort.Text = "Server Port:";
        lblServerPort.Location = new Point(330, 25);
        lblServerPort.AutoSize = true;

        nudServerPort = new NumericUpDown();
        nudServerPort.Location = new Point(410, 22);
        nudServerPort.Size = new Size(80, 23);
        nudServerPort.Minimum = 1;
        nudServerPort.Maximum = 65535;
        nudServerPort.Value = 10300;

        // Local Port
        lblLocalPort = new Label();
        lblLocalPort.Text = "Local Port:";
        lblLocalPort.Location = new Point(10, 60);
        lblLocalPort.AutoSize = true;

        nudLocalPort = new NumericUpDown();
        nudLocalPort.Location = new Point(110, 57);
        nudLocalPort.Size = new Size(80, 23);
        nudLocalPort.Minimum = 1;
        nudLocalPort.Maximum = 65535;
        nudLocalPort.Value = 10300;

        // Start/Stop Button
        btnStartStop = new Button();
        btnStartStop.Text = "Start";
        btnStartStop.Location = new Point(680, 40);
        btnStartStop.Size = new Size(80, 30);
        btnStartStop.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        // Add controls to Proxy Config GroupBox
        grpConfig.Controls.Add(lblServerAddress);
        grpConfig.Controls.Add(txtServerAddress);
        grpConfig.Controls.Add(lblServerPort);
        grpConfig.Controls.Add(nudServerPort);
        grpConfig.Controls.Add(lblLocalPort);
        grpConfig.Controls.Add(nudLocalPort);
        grpConfig.Controls.Add(btnStartStop);

        // GroupBox for File Lock
        grpFileLock = new GroupBox();
        grpFileLock.Text = "File Lock";
        grpFileLock.Location = new Point(12, 118);
        grpFileLock.Size = new Size(776, 60);
        grpFileLock.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        // File Path
        lblFilePath = new Label();
        lblFilePath.Text = "File:";
        lblFilePath.Location = new Point(10, 25);
        lblFilePath.AutoSize = true;

        txtFilePath = new TextBox();
        txtFilePath.Location = new Point(45, 22);
        txtFilePath.Size = new Size(480, 23);
        txtFilePath.ReadOnly = true;
        txtFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        // Browse Button
        btnBrowse = new Button();
        btnBrowse.Text = "Browse...";
        btnBrowse.Location = new Point(535, 21);
        btnBrowse.Size = new Size(75, 25);
        btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        // Lock/Unlock Button
        btnLock = new Button();
        btnLock.Text = "Lock";
        btnLock.Location = new Point(620, 21);
        btnLock.Size = new Size(65, 25);
        btnLock.Enabled = false;
        btnLock.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        // Lock Status Label
        lblLockStatus = new Label();
        lblLockStatus.Text = "";
        lblLockStatus.Location = new Point(695, 25);
        lblLockStatus.Size = new Size(70, 20);
        lblLockStatus.TextAlign = ContentAlignment.MiddleCenter;
        lblLockStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        // Add controls to File Lock GroupBox
        grpFileLock.Controls.Add(lblFilePath);
        grpFileLock.Controls.Add(txtFilePath);
        grpFileLock.Controls.Add(btnBrowse);
        grpFileLock.Controls.Add(btnLock);
        grpFileLock.Controls.Add(lblLockStatus);

        // Log Label
        lblLog = new Label();
        lblLog.Text = "Log:";
        lblLog.Location = new Point(12, 185);
        lblLog.AutoSize = true;

        // Log TextBox
        txtLog = new RichTextBox();
        txtLog.Location = new Point(12, 205);
        txtLog.Size = new Size(776, 335);
        txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtLog.ReadOnly = true;
        txtLog.BackColor = Color.Black;
        txtLog.ForeColor = Color.LightGreen;
        txtLog.Font = new Font("Consolas", 9F);

        // Status Strip
        statusStrip = new StatusStrip();
        lblStatus = new ToolStripStatusLabel();
        lblStatus.Text = "Status: Stopped";
        lblPacketCount = new ToolStripStatusLabel();
        lblPacketCount.Text = "Packets: 0";
        lblPacketCount.Alignment = ToolStripItemAlignment.Right;
        statusStrip.Items.Add(lblStatus);
        statusStrip.Items.Add(new ToolStripStatusLabel() { Spring = true });
        statusStrip.Items.Add(lblPacketCount);

        // Add controls to form
        this.Controls.Add(grpConfig);
        this.Controls.Add(grpFileLock);
        this.Controls.Add(lblLog);
        this.Controls.Add(txtLog);
        this.Controls.Add(statusStrip);
    }

    #endregion

    // Proxy Configuration
    private GroupBox grpConfig;
    private Label lblServerAddress;
    private TextBox txtServerAddress;
    private Label lblServerPort;
    private NumericUpDown nudServerPort;
    private Label lblLocalPort;
    private NumericUpDown nudLocalPort;
    private Button btnStartStop;

    // File Lock
    private GroupBox grpFileLock;
    private Label lblFilePath;
    private TextBox txtFilePath;
    private Button btnBrowse;
    private Button btnLock;
    private Label lblLockStatus;

    // Log
    private Label lblLog;
    private RichTextBox txtLog;

    // Status
    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblStatus;
    private ToolStripStatusLabel lblPacketCount;
}
