using System.Runtime.InteropServices;
using System.Text.Json;
using OpenSpeedTestClient.Core.Models;
using OpenSpeedTestClient.Core.Services;

namespace OpenSpeedTestClient;

static class Program
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(int dwProcessId);

    private const int ATTACH_PARENT_PROCESS = -1;

    [STAThread]
    static int Main(string[] args)
    {
        // Check if CLI mode - attach to console if needed
        var isCliMode = args.Length > 0 && (args.Contains("--cli") || args.Contains("-c"));
        if (isCliMode)
        {
            // Try to attach to parent console first (for piping), otherwise allocate new one
            if (!AttachConsole(ATTACH_PARENT_PROCESS))
            {
                AllocConsole();
            }
            
            // Reinitialize console streams after attaching
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
            Console.SetIn(new StreamReader(Console.OpenStandardInput()));
            
            return RunCliMode(args);
        }

        // GUI mode
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
        return 0;
    }

    static int RunCliMode(string[] args)
    {
        var verbose = args.Contains("--verbose") || args.Contains("-v");
        var configPath = GetArgValue(args, "--config");

        try
        {
            // Load configuration
            var configService = new ConfigService();
            var config = configService.LoadConfig(configPath);
            var httpClient = configService.CreateHttpClient(config);

            if (verbose)
            {
                Console.Error.WriteLine("OpenSpeedTest Client - CLI Mode");
                Console.Error.WriteLine($"Server: {config.ServerUrl}");
                Console.Error.WriteLine();
            }

            // Create progress reporter for verbose mode
            IProgress<TestProgress>? progress = null;
            if (verbose)
            {
                progress = new Progress<TestProgress>(p =>
                {
                    Console.Error.WriteLine($"[{p.Phase}] {p.Status}");
                });
            }

            // Run test
            var runner = new SpeedTestRunner(config, httpClient);
            var result = runner.RunTestAsync(progress).GetAwaiter().GetResult();

            // Output JSON result to stdout
            var json = JsonSerializer.Serialize(result, SpeedTestJsonContext.Default.SpeedTestResult);
            Console.WriteLine(json);

            return result.Success ? 0 : 1;
        }
        catch (Exception ex)
        {
            var errorResult = new SpeedTestResult
            {
                Success = false,
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(errorResult, SpeedTestJsonContext.Default.SpeedTestResult);
            Console.WriteLine(json);

            if (verbose)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }

            return 1;
        }
    }

    static string? GetArgValue(string[] args, string argName)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == argName)
            {
                return args[i + 1];
            }
        }
        return null;
    }
}
