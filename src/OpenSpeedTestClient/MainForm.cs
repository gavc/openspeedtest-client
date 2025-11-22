using OpenSpeedTestClient.Core.Models;
using OpenSpeedTestClient.Core.Services;

namespace OpenSpeedTestClient;

public partial class MainForm : Form
{
    private SpeedTestConfig? _config;
    private HttpClient? _httpClient;
    private CancellationTokenSource? _cts;
    private bool _isRunning;

    public MainForm()
    {
        InitializeComponent();
        LoadConfiguration();
    }

    private void LoadConfiguration()
    {
        try
        {
            var configService = new ConfigService();
            _config = configService.LoadConfig();
            _httpClient = configService.CreateHttpClient(_config);

            lblServerValue.Text = _config.TestServerUrl;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load configuration: {ex.Message}", "Configuration Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }

    private async void btnStart_Click(object sender, EventArgs e)
    {
        if (_isRunning)
        {
            // Stop test
            _cts?.Cancel();
            return;
        }

        if (_config == null || _httpClient == null)
        {
            MessageBox.Show("Configuration not loaded", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _isRunning = true;
        btnStart.Text = "Stop Test";
        progressBar.Value = 0;
        ClearResults();

        _cts = new CancellationTokenSource();

        try
        {
            var progress = new Progress<TestProgress>(UpdateProgress);
            var runner = new SpeedTestRunner(_config, _httpClient);
            var result = await runner.RunTestAsync(progress, _cts.Token);

            if (result.Success)
            {
                DisplayResults(result);
            }
            else
            {
                MessageBox.Show($"Test failed: {result.Error}", "Test Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (OperationCanceledException)
        {
            lblStatus.Text = "Test cancelled";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _isRunning = false;
            btnStart.Text = "Start Test";
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void UpdateProgress(TestProgress progress)
    {
        if (InvokeRequired)
        {
            Invoke(() => UpdateProgress(progress));
            return;
        }

        progressBar.Value = Math.Min((int)progress.Progress, 100);
        lblStatus.Text = progress.Status;

        // Update current speed display
        if (progress.Phase == TestPhase.Download || progress.Phase == TestPhase.Upload)
        {
            lblCurrentSpeed.Text = $"{progress.CurrentSpeed:F2} Mbps";
        }
    }

    private void DisplayResults(SpeedTestResult result)
    {
        lblComputerNameValue.Text = result.ComputerName;
        lblIPValue.Text = result.IP;
        lblConnectionTypeValue.Text = result.ConnectionType;
        lblDownloadValue.Text = $"{result.DownloadMbps:F2} Mbps";
        lblUploadValue.Text = $"{result.UploadMbps:F2} Mbps";
        lblPingValue.Text = $"{result.PingMs:F1} ms";
        lblJitterValue.Text = $"{result.JitterMs:F1} ms";
        lblStatus.Text = "Test complete!";
        progressBar.Value = 100;
    }

    private void ClearResults()
    {
        lblComputerNameValue.Text = "-";
        lblIPValue.Text = "-";
        lblConnectionTypeValue.Text = "-";
        lblDownloadValue.Text = "-";
        lblUploadValue.Text = "-";
        lblPingValue.Text = "-";
        lblJitterValue.Text = "-";
        lblCurrentSpeed.Text = "-";
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _cts?.Cancel();
        _httpClient?.Dispose();
        base.OnFormClosing(e);
    }

    private void OnAboutClicked()
    {
        try
        {
            using var about = new AboutForm();
            about.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unable to open About dialog: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
