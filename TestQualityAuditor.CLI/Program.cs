using CommandLine;
using Spectre.Console;
using TestQualityAuditor.Core;
using TestQualityAuditor.Core.Models;

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

            var analyzer = new TestQualityAnalyzer();
            var result = await analyzer.AnalyzeProject(options.ProjectPath);

            MostrarResultados(result);

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }

    private static void MostrarResultados(ProjectAnalysisResult result)
    {
        AnsiConsole.MarkupLine($"\n[bold green]📊 Analysis Results[/]");
        AnsiConsole.MarkupLine($"[dim]Project: {result.ProjectPath}[/]");
        AnsiConsole.MarkupLine($"[dim]Analyzed: {result.AnalyzedAt:yyyy-MM-dd HH:mm:ss}[/]\n");

        // General metrics
        var generalTable = new Table()
            .AddColumn("Metric")
            .AddColumn("Score")
            .AddColumn("Status");

        generalTable.AddRow("Overall Score",
            $"{result.AverageScore:F1}/10",
            GetStatusEmoji(result.AverageScore));

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
}

public class Options
{
    [Option('p', "project", Required = true, HelpText = "Path to the .csproj file of the test project")]
    public string ProjectPath { get; set; } = string.Empty;

    [Option('o', "output", HelpText = "Output file for the report (optional)")]
    public string? OutputFile { get; set; }

    [Option('v', "verbose", HelpText = "Show detailed information")]
    public bool Verbose { get; set; }
}
