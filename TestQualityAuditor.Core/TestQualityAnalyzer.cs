using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestQualityAuditor.Core.Analyzers;
using TestQualityAuditor.Core.Models;

namespace TestQualityAuditor.Core;

public class TestQualityAnalyzer
{
    private readonly CompletenessAnalyzer _completenessAnalyzer;
    private readonly CorrectnessAnalyzer _correctnessAnalyzer;
    private readonly MaintainabilityAnalyzer _maintainabilityAnalyzer;

    public TestQualityAnalyzer()
    {
        _completenessAnalyzer = new CompletenessAnalyzer();
        _correctnessAnalyzer = new CorrectnessAnalyzer();
        _maintainabilityAnalyzer = new MaintainabilityAnalyzer();
    }

    public async Task<ProjectAnalysisResult> AnalyzeProject(string projectPath)
    {
        var result = new ProjectAnalysisResult
        {
            ProjectPath = projectPath
        };

        // Get all C# files from the project directory
        var projectDir = Path.GetDirectoryName(projectPath) ?? projectPath;
        var csFiles = Directory.GetFiles(projectDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\bin\\") && !f.Contains("\\obj\\"))
            .ToList();

        var testMethods = new List<MethodDeclarationSyntax>();

        foreach (var file in csFiles)
        {
            var fileContent = await File.ReadAllTextAsync(file);
            var syntaxTree = CSharpSyntaxTree.ParseText(fileContent);
            var root = syntaxTree.GetRoot();

            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .Where(m => IsTestMethod(m))
                .ToList();

            testMethods.AddRange(methods);
        }

        // Create a simple compilation for semantic analysis
        var compilation = CSharpCompilation.Create("TempAssembly")
            .AddSyntaxTrees(csFiles.Select(f => CSharpSyntaxTree.ParseText(File.ReadAllTextAsync(f).Result)));

        foreach (var testMethod in testMethods)
        {
            var testResult = AnalyzeTestMethod(testMethod, compilation);
            result.TestResults.Add(testResult);
        }

        // Calculate general metrics
        result.AverageScore = result.TestResults.Any()
            ? result.TestResults.Average(t => t.Metrics.OverallScore)
            : 0;

        result.Recommendations = GenerateRecommendations(result.TestResults);

        return result;
    }

    private bool IsTestMethod(MethodDeclarationSyntax method)
    {
        // Check for test attributes
        var attributes = method.AttributeLists
            .SelectMany(al => al.Attributes)
            .Select(a => a.Name.ToString());

        var hasTestAttribute = attributes.Any(attr =>
            attr.Contains("Test") ||
            attr.Contains("Fact") ||
            attr.Contains("Theory"));

        // Check for test naming patterns
        var methodName = method.Identifier.ValueText;
        var hasTestName = methodName.StartsWith("Test") ||
                         methodName.StartsWith("Should") ||
                         methodName.StartsWith("When") ||
                         methodName.StartsWith("Given");

        return hasTestAttribute || hasTestName;
    }

    private TestAnalysisResult AnalyzeTestMethod(MethodDeclarationSyntax method, Compilation compilation)
    {
        // Get class name from the containing class
        var classDeclaration = method.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        var className = classDeclaration?.Identifier.ValueText ?? "Unknown";

        var result = new TestAnalysisResult
        {
            TestMethodName = method.Identifier.ValueText,
            TestClassName = className
        };

        // Create a simple semantic model (we'll pass null since we're doing syntax-only analysis)
        SemanticModel? semanticModel = null;

        // Analyze each dimension
        result.Metrics.Completeness = _completenessAnalyzer.AnalyzeCompleteness(method, semanticModel);
        result.Metrics.Correctness = _correctnessAnalyzer.AnalyzeCorrectness(method, semanticModel);
        result.Metrics.Maintainability = _maintainabilityAnalyzer.AnalyzeMaintainability(method, semanticModel);

        // Calculate overall score (weighted average)
        result.Metrics.OverallScore =
            (result.Metrics.Completeness * 0.4m) +
            (result.Metrics.Correctness * 0.4m) +
            (result.Metrics.Maintainability * 0.2m);

        return result;
    }

    private List<string> GenerateRecommendations(List<TestAnalysisResult> testResults)
    {
        var recomendaciones = new List<string>();

        if (!testResults.Any())
        {
            recomendaciones.Add("No test methods found in this project.");
            return recomendaciones;
        }

        var averageCompleteness = testResults.Average(t => t.Metrics.Completeness);
        var averageCorrectness = testResults.Average(t => t.Metrics.Correctness);
        var averageMaintainability = testResults.Average(t => t.Metrics.Maintainability);

        if (averageCompleteness < 6)
            recomendaciones.Add("Improve edge case coverage and error scenarios");

        if (averageCorrectness < 6)
            recomendaciones.Add("Review trivial assertions and reduce mock abuse");

        if (averageMaintainability < 6)
            recomendaciones.Add("Simplify complex tests and reduce code duplication");

        var problematicTests = testResults.Where(t => t.Metrics.OverallScore < 5).ToList();
        if (problematicTests.Any())
        {
            recomendaciones.Add($"Review {problematicTests.Count} tests with low scores");
        }

        return recomendaciones;
    }
}
