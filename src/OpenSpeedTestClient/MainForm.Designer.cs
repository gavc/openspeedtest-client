namespace OpenSpeedTestClient;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel mainLayout = null!;
    private GroupBox grpServer = null!;
    private Label lblServer = null!;
    private Label lblServerValue = null!;
    private Button btnStart = null!;
    private ProgressBar progressBar = null!;
    private Label lblStatus = null!;
    private Label lblCurrentSpeed = null!;
    private GroupBox grpResults = null!;
    private TableLayoutPanel resultsLayout = null!;
    private Label lblComputerName = null!;
    private Label lblComputerNameValue = null!;
    private Label lblIP = null!;
    private Label lblIPValue = null!;
    private Label lblConnectionType = null!;
    private Label lblConnectionTypeValue = null!;
    private Label lblDownload = null!;
    private Label lblDownloadValue = null!;
    private Label lblUpload = null!;
    private Label lblUploadValue = null!;
    private Label lblPing = null!;
    private Label lblPingValue = null!;
    private Label lblJitter = null!;
    private Label lblJitterValue = null!;
    private LinkLabel linkAbout = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.AutoScaleMode = AutoScaleMode.Dpi;
        this.ClientSize = new Size(640, 560);
        this.Text = "OpenSpeedTest Client";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MinimumSize = new Size(640, 560);
        this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        // Main layout
        mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(10)
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // Server group
        grpServer = new GroupBox
        {
            Text = "Server Configuration",
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(10)
        };

        var serverLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = true
        };
        serverLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        serverLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        lblServer = new Label
        {
            Text = "Server URL:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Padding = new Padding(0, 3, 10, 0)
        };

        lblServerValue = new Label
        {
            Text = "-",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Padding = new Padding(0, 3, 0, 0),
            Font = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Bold)
        };

        serverLayout.Controls.Add(lblServer, 0, 0);
        serverLayout.Controls.Add(lblServerValue, 1, 0);
        grpServer.Controls.Add(serverLayout);

        // Start button
        btnStart = new Button
        {
            Text = "Start Test",
            Height = 40,
            Dock = DockStyle.Fill,
            Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
        };
        btnStart.Click += btnStart_Click;

        // Progress bar
        progressBar = new ProgressBar
        {
            Dock = DockStyle.Fill,
            Height = 30
        };

        // Status labels
        var statusLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = true
        };
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

        lblStatus = new Label
        {
            Text = "Ready",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Padding = new Padding(0, 5, 0, 0)
        };

        lblCurrentSpeed = new Label
        {
            Text = "-",
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            Padding = new Padding(0, 5, 0, 0),
            Font = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Bold)
        };

        statusLayout.Controls.Add(lblStatus, 0, 0);
        statusLayout.Controls.Add(lblCurrentSpeed, 1, 0);

        // Results group
        grpResults = new GroupBox
        {
            Text = "Test Results",
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };

        resultsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            AutoSize = true
        };
        resultsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        resultsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));

        for (int i = 0; i < 7; i++)
        {
            resultsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        // Computer Name
        lblComputerName = new Label { Text = "Computer Name:", AutoSize = true, Padding = new Padding(0, 5, 0, 5) };
        lblComputerNameValue = new Label { Text = "-", AutoSize = true, Padding = new Padding(0, 5, 0, 5), Font = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Bold) };

        // IP Address
        lblIP = new Label { Text = "IP Address:", AutoSize = true, Padding = new Padding(0, 5, 0, 5) };
        lblIPValue = new Label { Text = "-", AutoSize = true, Padding = new Padding(0, 5, 0, 5), Font = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Bold) };

        // Connection Type
        lblConnectionType = new Label { Text = "Connection Type:", AutoSize = true, Padding = new Padding(0, 5, 0, 5) };
        lblConnectionTypeValue = new Label { Text = "-", AutoSize = true, Padding = new Padding(0, 5, 0, 5), Font = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Bold) };

        // Download Speed
        lblDownload = new Label { Text = "Download Speed:", AutoSize = true, Padding = new Padding(0, 5, 0, 5) };
        lblDownloadValue = new Label { Text = "-", AutoSize = true, Padding = new Padding(0, 5, 0, 5), Font = new Font(this.Font.FontFamily, this.Font.Size + 2, FontStyle.Bold), ForeColor = Color.Green };

        // Upload Speed
        lblUpload = new Label { Text = "Upload Speed:", AutoSize = true, Padding = new Padding(0, 5, 0, 5) };
        lblUploadValue = new Label { Text = "-", AutoSize = true, Padding = new Padding(0, 5, 0, 5), Font = new Font(this.Font.FontFamily, this.Font.Size + 2, FontStyle.Bold), ForeColor = Color.Blue };

        // Ping
        lblPing = new Label { Text = "Ping:", AutoSize = true, Padding = new Padding(0, 5, 0, 5) };
        lblPingValue = new Label { Text = "-", AutoSize = true, Padding = new Padding(0, 5, 0, 5), Font = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Bold) };

        // Jitter
        lblJitter = new Label { Text = "Jitter:", AutoSize = true, Padding = new Padding(0, 5, 0, 5) };
        lblJitterValue = new Label { Text = "-", AutoSize = true, Padding = new Padding(0, 5, 0, 5), Font = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Bold) };

        resultsLayout.Controls.Add(lblComputerName, 0, 0);
        resultsLayout.Controls.Add(lblComputerNameValue, 1, 0);
        resultsLayout.Controls.Add(lblIP, 0, 1);
        resultsLayout.Controls.Add(lblIPValue, 1, 1);
        resultsLayout.Controls.Add(lblConnectionType, 0, 2);
        resultsLayout.Controls.Add(lblConnectionTypeValue, 1, 2);
        resultsLayout.Controls.Add(lblDownload, 0, 3);
        resultsLayout.Controls.Add(lblDownloadValue, 1, 3);
        resultsLayout.Controls.Add(lblUpload, 0, 4);
        resultsLayout.Controls.Add(lblUploadValue, 1, 4);
        resultsLayout.Controls.Add(lblPing, 0, 5);
        resultsLayout.Controls.Add(lblPingValue, 1, 5);
        resultsLayout.Controls.Add(lblJitter, 0, 6);
        resultsLayout.Controls.Add(lblJitterValue, 1, 6);

        grpResults.Controls.Add(resultsLayout);

        // Add to main layout
        mainLayout.Controls.Add(grpServer, 0, 0);
        mainLayout.Controls.Add(btnStart, 0, 1);
        mainLayout.Controls.Add(progressBar, 0, 2);
        mainLayout.Controls.Add(statusLayout, 0, 3);
        mainLayout.Controls.Add(grpResults, 0, 4);

        // About link
        linkAbout = new LinkLabel
        {
            Text = "About",
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            Padding = new Padding(0, 6, 0, 0)
        };
        linkAbout.LinkClicked += (s, e) => { OnAboutClicked(); };

        mainLayout.Controls.Add(linkAbout, 0, 5);

        this.Controls.Add(mainLayout);
    }
}
