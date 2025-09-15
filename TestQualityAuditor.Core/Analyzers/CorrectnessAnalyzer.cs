using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestQualityAuditor.Core.Models;

namespace TestQualityAuditor.Core.Analyzers;

public class CorrectnessAnalyzer
{
    public decimal AnalyzeCorrectness(MethodDeclarationSyntax testMethod, SemanticModel? semanticModel)
    {
        var score = 10m;
        var problems = new List<string>();

        // 1. Detect trivial or false assertions
        if (HasTrivialAssertions(testMethod))
        {
            score -= 4m;
            problems.Add("Trivial assertions detected");
        }

        // 2. Check mock abuse
        var mockRatio = CalculateMockRatio(testMethod);
        if (mockRatio > 0.7m)
        {
            score -= 3m;
            problems.Add($"Mock abuse detected ({mockRatio:P0})");
        }

        // 3. Check if test actually validates behavior
        if (!ValidatesRealBehavior(testMethod))
        {
            score -= 3m;
            problems.Add("Test does not validate real code behavior");
        }

        // 4. Detect tests that never fail
        if (TestNeverFails(testMethod))
        {
            score -= 5m;
            problems.Add("Test designed to never fail");
        }

        return Math.Max(0, Math.Min(10, score));
    }

    private bool HasTrivialAssertions(MethodDeclarationSyntax method)
    {
        var assertions = method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(inv => inv.Expression.ToString().Contains("Assert"));

        foreach (var assertion in assertions)
        {
            var texto = assertion.ToString();

            // Detect trivial assertions
            if (texto.Contains("Assert.True(true)") ||
                texto.Contains("Assert.False(false)") ||
                texto.Contains("Assert.NotNull(null)") ||
                texto.Contains("Assert.AreEqual(1, 1)") ||
                texto.Contains("Assert.IsTrue(1 == 1)"))
            {
                return true;
            }
        }

        return false;
    }

    private decimal CalculateMockRatio(MethodDeclarationSyntax method)
    {
        var texto = method.ToString();
        var totalCalls = CountMethodCalls(texto);
        var mockCalls = CountMockCalls(texto);

        if (totalCalls == 0) return 0;
        return (decimal)mockCalls / totalCalls;
    }

    private int CountMethodCalls(string codigo)
    {
        return codigo.Split('.').Length - 1; // Simple approximation
    }

    private int CountMockCalls(string codigo)
    {
        var mockWords = new[] { "Mock", "Setup", "Verify", "It.IsAny", "Returns", "Callback" };
        return mockWords.Sum(word => (codigo.Split(word).Length - 1));
    }

    private bool ValidatesRealBehavior(MethodDeclarationSyntax method)
    {
        var texto = method.ToString();

        // If only has mocks and doesn't call real method, doesn't validate behavior
        if (texto.Contains("Mock") && !texto.Contains("new ") && !texto.Contains("actual"))
        {
            return false;
        }

        // Must have at least one assertion that is not just verifying mocks
        var assertions = method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(inv => inv.Expression.ToString().Contains("Assert"))
            .ToList();

        return assertions.Any(a => !a.ToString().Contains("Verify"));
    }

    private bool TestNeverFails(MethodDeclarationSyntax method)
    {
        var texto = method.ToString();

        // Patterns that indicate the test never fails
        return texto.Contains("Assert.DoesNotThrow") ||
               texto.Contains("Assert.True(true)") ||
               (texto.Contains("try") && texto.Contains("catch") && !texto.Contains("Assert"));
    }
}
