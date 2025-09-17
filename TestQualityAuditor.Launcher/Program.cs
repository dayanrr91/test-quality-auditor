using System.Diagnostics;

namespace TestQualityAuditor.Launcher;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🔍 Test Quality Auditor - Unified Launcher");
        Console.WriteLine("==========================================");
        Console.WriteLine();

        if (args.Length == 0)
        {
            Console.WriteLine("❌ Error: Se requiere especificar la ruta del proyecto a analizar");
            Console.WriteLine();
            Console.WriteLine("💡 Uso:");
            Console.WriteLine("   dotnet run --project TestQualityAuditor.Launcher -- \"C:\\ruta\\a\\tu\\proyecto\"");
            Console.WriteLine();
            Console.WriteLine("🎯 Ejemplos:");
            Console.WriteLine("   dotnet run --project TestQualityAuditor.Launcher -- \"D:\\Work\\tam\\SampleTestProject\"");
            Console.WriteLine("   dotnet run --project TestQualityAuditor.Launcher -- \"C:\\MiApp\\Tests\"");
            return;
        }

        var projectPath = args[0];
        Console.WriteLine($"📁 Proyecto a analizar: {projectPath}");
        Console.WriteLine();

        try
        {
            // Paso 1: Ejecutar análisis CLI
            Console.WriteLine("🔍 Paso 1: Ejecutando análisis de calidad de tests...");
            await RunCliAnalysis(projectPath);
            Console.WriteLine("✅ Análisis completado! Reporte generado: reporte.json");
            Console.WriteLine();

            // Paso 2: Iniciar aplicación web
            Console.WriteLine("🚀 Paso 2: Iniciando interfaz web...");
            var webProcess = StartWebApplication();
            Console.WriteLine("✅ Interfaz web iniciada!");

            // Esperar un momento para que la web se inicie
            Console.WriteLine("⏳ Esperando que la interfaz web se inicialice...");
            await Task.Delay(3000);

            // Abrir navegador
            OpenBrowser("http://localhost:5000");

            Console.WriteLine();
            Console.WriteLine("🎉 ¡Todo listo!");
            Console.WriteLine("📊 El reporte se cargó automáticamente en: http://localhost:5000");
            Console.WriteLine("🧪 Revisa las tabs de Unit Tests e Integration Tests");
            Console.WriteLine();
            Console.WriteLine("Presiona Ctrl+C para detener la aplicación...");

            // Esperar a que termine el proceso web
            await webProcess.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine("💡 Intenta ejecutar manualmente:");
            Console.WriteLine($"   dotnet run --project TestQualityAuditor.CLI -- --project \"{projectPath}\" --output \"reporte.json\"");
            Console.WriteLine("   dotnet run --project TestQualityAuditor.Web");
        }
    }

    static async Task RunCliAnalysis(string projectPath)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var cliProjectPath = Path.GetFullPath(Path.Combine(currentDir, "TestQualityAuditor.CLI", "TestQualityAuditor.CLI.csproj"));

        if (!File.Exists(cliProjectPath))
        {
            throw new FileNotFoundException($"CLI project not found at: {cliProjectPath}");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{cliProjectPath}\" -- --project \"{projectPath}\" --output \"reporte.json\"",
            WorkingDirectory = currentDir,
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = false
        };

        var process = Process.Start(startInfo);

        if (process == null)
        {
            throw new InvalidOperationException("Failed to start the CLI analysis");
        }

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"CLI analysis failed with exit code: {process.ExitCode}");
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
            WorkingDirectory = currentDir, // Cambiar para que ejecute desde el directorio raíz
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
