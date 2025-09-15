using System.Diagnostics;

namespace TestQualityAuditor.Launcher;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 Starting Test Quality Auditor Web UI...");
        Console.WriteLine();

        try
        {
            // Start the web application
            var webProcess = StartWebApplication();
            Console.WriteLine("✅ Web application started successfully!");

            // Wait a moment for the web app to start
            Console.WriteLine("⏳ Waiting for web application to initialize...");
            await Task.Delay(5000);

            // Open the browser
            OpenBrowser("http://localhost:5000");

            Console.WriteLine("🌐 Web UI opened in your default browser!");
            Console.WriteLine("📊 Navigate to: http://localhost:5000");
            Console.WriteLine();
            Console.WriteLine("Press Ctrl+C to stop the application...");

            // Wait for the web process to finish or for user to press Ctrl+C
            await webProcess.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine("💡 Try running the web application directly:");
            Console.WriteLine("   dotnet run --project TestQualityAuditor.Web");
        }
    }

    static Process StartWebApplication()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var webProjectPath = Path.GetFullPath(Path.Combine(currentDir, "TestQualityAuditor.Web", "TestQualityAuditor.Web.csproj"));

        if (!File.Exists(webProjectPath))
        {
            throw new FileNotFoundException($"Web project not found at: {webProjectPath}");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{webProjectPath}\"",
            WorkingDirectory = Path.GetDirectoryName(webProjectPath),
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = false
        };

        var process = Process.Start(startInfo);

        if (process == null)
        {
            throw new InvalidOperationException("Failed to start the web application");
        }

        return process;
    }

    static void OpenBrowser(string url)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Could not open browser automatically: {ex.Message}");
            Console.WriteLine($"Please manually open: {url}");
        }
    }
}
