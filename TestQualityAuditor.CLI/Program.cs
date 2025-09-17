using CommandLine;
using Spectre.Console;
using TestQualityAuditor.Core;
using TestQualityAuditor.Core.Models;
using TestQualityAuditor.Web.Services;

namespace TestQualityAuditor.CLI;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var result = Parser.Default.ParseArguments<Options>(args);
        return await result.MapResult(
            async options => await RunAnalysis(options),
            errors => Task.FromResult(1)
        );
    }

    private static async Task<int> RunAnalysis(Options options)
    {
        try
        {
            AnsiConsole.MarkupLine("[bold blue]🔍 Analyzing test quality...[/]");

            if (File.Exists(options.ProjectPath) && options.ProjectPath.EndsWith(".csproj"))
            {
                // Analizar proyecto específico
                await AnalyzeSingleProject(options.ProjectPath, options);
            }
            else if (Directory.Exists(options.ProjectPath))
            {
                // Analizar todos los proyectos de test en el directorio
                await AnalyzeDirectory(options.ProjectPath, options);
            }
            else
            {
                AnsiConsole.MarkupLine("[red]❌ Error: La ruta especificada no existe o no es válida[/]");
                AnsiConsole.MarkupLine("[yellow]💡 Ejemplos de uso:[/]");
                AnsiConsole.MarkupLine("  • Proyecto específico: [green]--project C:\\MiApp\\Tests\\MyApp.Tests.csproj[/]");
                AnsiConsole.MarkupLine("  • Directorio completo: [green]--project C:\\MiApp\\[/]");
                return 1;
            }

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }

    private static async Task AnalyzeSingleProject(string projectPath, Options options)
    {
        var analyzer = new TestQualityAnalyzer();
        var result = await analyzer.AnalyzeProject(projectPath);

        // Determinar tipo de test
        var projectDiscovery = new ProjectDiscoveryService();
        result.TestType = projectDiscovery.DetermineTestType(projectPath);

        MostrarResultados(result);

        if (!string.IsNullOrEmpty(options.OutputFile))
        {
            await GuardarReporte(result, options.OutputFile);
        }
    }

    private static async Task AnalyzeDirectory(string directoryPath, Options options)
    {
        var projectDiscovery = new ProjectDiscoveryService();
        var testProjects = projectDiscovery.DiscoverTestProjects(directoryPath);

        if (!testProjects.Any())
        {
            AnsiConsole.MarkupLine("[yellow]⚠️ No se encontraron proyectos de test en el directorio especificado[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[green]✅ Encontrados {testProjects.Count} proyecto(s) de test[/]\n");

        var analyzer = new TestQualityAnalyzer();
        var allResults = new List<ProjectAnalysisResult>();

        foreach (var projectPath in testProjects)
        {
            var projectName = Path.GetFileNameWithoutExtension(projectPath);
            AnsiConsole.MarkupLine($"[dim]Analizando: {projectName}...[/]");

            try
            {
                var result = await analyzer.AnalyzeProject(projectPath);
                result.TestType = projectDiscovery.DetermineTestType(projectPath);
                allResults.Add(result);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]❌ Error analizando {projectName}: {ex.Message}[/]");
            }
        }

        MostrarResultadosMultiples(allResults);

        if (!string.IsNullOrEmpty(options.OutputFile))
        {
            await GuardarReporteMultiple(allResults, options.OutputFile);
        }
    }

    private static void MostrarResultados(ProjectAnalysisResult result)
    {
        var projectName = Path.GetFileNameWithoutExtension(result.ProjectPath);
        var testTypeEmoji = result.TestType == "Unit" ? "🧪" : "🔗";

        AnsiConsole.MarkupLine($"\n[bold green]📊 Analysis Results[/]");
        AnsiConsole.MarkupLine($"[dim]Project: {projectName} {testTypeEmoji} {result.TestType} Tests[/]");
        AnsiConsole.MarkupLine($"[dim]Path: {result.ProjectPath}[/]");
        AnsiConsole.MarkupLine($"[dim]Analyzed: {result.AnalyzedAt:yyyy-MM-dd HH:mm:ss}[/]\n");

        // General metrics
        var generalTable = new Table()
            .AddColumn("Metric")
            .AddColumn("Score")
            .AddColumn("Status");

        generalTable.AddRow("Overall Score",
            $"{result.AverageScore:F1}/10",
            GetStatusEmoji(result.AverageScore));

        generalTable.AddRow("Tests Found",
            result.TestResults.Count.ToString(),
            result.TestResults.Count > 0 ? "✅" : "⚠️");

        AnsiConsole.Write(generalTable);

        // Tests individuales
        if (result.TestResults.Any())
        {
            AnsiConsole.MarkupLine("\n[bold]📋 Analyzed Tests:[/]");

            var testsTable = new Table()
                .AddColumn("Test")
                .AddColumn("Completeness")
                .AddColumn("Correctness")
                .AddColumn("Maintainability")
                .AddColumn("Overall");

            foreach (var test in result.TestResults.OrderByDescending(t => t.Metrics.OverallScore))
            {
                testsTable.AddRow(
                    $"{test.TestClassName}.{test.TestMethodName}",
                    $"{test.Metrics.Completeness:F1}",
                    $"{test.Metrics.Correctness:F1}",
                    $"{test.Metrics.Maintainability:F1}",
                    $"{test.Metrics.OverallScore:F1}"
                );
            }

            AnsiConsole.Write(testsTable);
        }

        // Recommendations
        if (result.Recommendations.Any())
        {
            AnsiConsole.MarkupLine("\n[bold yellow]💡 Recommendations:[/]");
            foreach (var recommendation in result.Recommendations)
            {
                AnsiConsole.MarkupLine($"• {recommendation}");
            }
        }

        // Problematic tests
        var problematicTests = result.TestResults.Where(t => t.Metrics.OverallScore < 5).ToList();
        if (problematicTests.Any())
        {
            AnsiConsole.MarkupLine("\n[bold red]⚠️ Tests that need attention:[/]");
            foreach (var test in problematicTests)
            {
                AnsiConsole.MarkupLine($"[red]• {test.TestClassName}.{test.TestMethodName} ({test.Metrics.OverallScore:F1}/10)[/]");
            }
        }
    }

    private static string GetStatusEmoji(decimal score)
    {
        return score switch
        {
            >= 8 => "🟢 Excellent",
            >= 6 => "🟡 Good",
            >= 4 => "🟠 Fair",
            _ => "🔴 Needs improvement"
        };
    }

    private static void MostrarResultadosMultiples(List<ProjectAnalysisResult> results)
    {
        if (!results.Any())
        {
            AnsiConsole.MarkupLine("[yellow]⚠️ No se pudieron analizar proyectos[/]");
            return;
        }

        AnsiConsole.MarkupLine($"\n[bold green]📊 Resumen General[/]");

        // Resumen por tipo de test
        var unitTests = results.Where(r => r.TestType == "Unit").ToList();
        var integrationTests = results.Where(r => r.TestType == "Integration").ToList();

        var summaryTable = new Table()
            .AddColumn("Tipo de Test")
            .AddColumn("Proyectos")
            .AddColumn("Tests Totales")
            .AddColumn("Promedio Score");

        if (unitTests.Any())
        {
            var totalUnitTests = unitTests.Sum(r => r.TestResults.Count);
            var avgUnitScore = unitTests.Average(r => r.AverageScore);
            summaryTable.AddRow("🧪 Unit Tests",
                unitTests.Count.ToString(),
                totalUnitTests.ToString(),
                $"{avgUnitScore:F1}/10");
        }

        if (integrationTests.Any())
        {
            var totalIntegrationTests = integrationTests.Sum(r => r.TestResults.Count);
            var avgIntegrationScore = integrationTests.Average(r => r.AverageScore);
            summaryTable.AddRow("🔗 Integration Tests",
                integrationTests.Count.ToString(),
                totalIntegrationTests.ToString(),
                $"{avgIntegrationScore:F1}/10");
        }

        AnsiConsole.Write(summaryTable);

        // Mostrar cada proyecto individualmente
        foreach (var result in results.OrderBy(r => r.TestType).ThenBy(r => Path.GetFileNameWithoutExtension(r.ProjectPath)))
        {
            MostrarResultados(result);
        }
    }

    private static async Task GuardarReporte(ProjectAnalysisResult result, string outputFile)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(outputFile, json);
            AnsiConsole.MarkupLine($"[green]✅ Reporte guardado en: {outputFile}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ Error guardando reporte: {ex.Message}[/]");
        }
    }

    private static async Task GuardarReporteMultiple(List<ProjectAnalysisResult> results, string outputFile)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(results, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(outputFile, json);
            AnsiConsole.MarkupLine($"[green]✅ Reporte guardado en: {outputFile}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ Error guardando reporte: {ex.Message}[/]");
        }
    }
}

public class Options
{
    [Option('p', "project", Required = true, HelpText = "Path to a .csproj file or directory containing test projects")]
    public string ProjectPath { get; set; } = string.Empty;

    [Option('o', "output", HelpText = "Output file for the JSON report (optional)")]
    public string? OutputFile { get; set; }

    [Option('v', "verbose", HelpText = "Show detailed information")]
    public bool Verbose { get; set; }
}
