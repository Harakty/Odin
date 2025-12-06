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
        this.ClientSize = new System.Drawing.Size(800, 550);
        this.Text = "Odin - DAoC Proxy";
        this.MinimumSize = new System.Drawing.Size(600, 400);

        // GroupBox for configuration
        grpConfig = new GroupBox();
        grpConfig.Text = "Configuration";
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

        // Add controls to GroupBox
        grpConfig.Controls.Add(lblServerAddress);
        grpConfig.Controls.Add(txtServerAddress);
        grpConfig.Controls.Add(lblServerPort);
        grpConfig.Controls.Add(nudServerPort);
        grpConfig.Controls.Add(lblLocalPort);
        grpConfig.Controls.Add(nudLocalPort);
        grpConfig.Controls.Add(btnStartStop);

        // Log TextBox
        lblLog = new Label();
        lblLog.Text = "Log:";
        lblLog.Location = new Point(12, 120);
        lblLog.AutoSize = true;

        txtLog = new RichTextBox();
        txtLog.Location = new Point(12, 140);
        txtLog.Size = new Size(776, 330);
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
        this.Controls.Add(lblLog);
        this.Controls.Add(txtLog);
        this.Controls.Add(statusStrip);
    }

    #endregion

    private GroupBox grpConfig;
    private Label lblServerAddress;
    private TextBox txtServerAddress;
    private Label lblServerPort;
    private NumericUpDown nudServerPort;
    private Label lblLocalPort;
    private NumericUpDown nudLocalPort;
    private Button btnStartStop;
    private Label lblLog;
    private RichTextBox txtLog;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblStatus;
    private ToolStripStatusLabel lblPacketCount;
}
