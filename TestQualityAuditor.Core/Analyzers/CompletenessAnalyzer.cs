using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestQualityAuditor.Core.Models;

namespace TestQualityAuditor.Core.Analyzers;

public class CompletenessAnalyzer
{
    public decimal AnalyzeCompleteness(MethodDeclarationSyntax testMethod, SemanticModel? semanticModel)
    {
        var score = 10m;
        var problems = new List<string>();

        // 1. Detect if only has happy path
        if (OnlyHappyPath(testMethod))
        {
            score -= 4m;
            problems.Add("Test only covers happy path");
        }

        // 2. Check edge cases (null, empty, boundary values)
        var edgeCases = CountEdgeCases(testMethod);
        if (edgeCases == 0)
        {
            score -= 3m;
            problems.Add("No edge cases detected");
        }
        else if (edgeCases >= 3)
        {
            score += 1m; // Bonus for edge cases
        }

        // 3. Check multiple assertions
        var assertions = CountAssertions(testMethod);
        if (assertions < 2)
        {
            score -= 2m;
            problems.Add("Too few assertions to validate completely");
        }

        // 4. Check if tests exceptions
        if (!TestsExceptions(testMethod))
        {
            score -= 1m;
            problems.Add("No exception cases tested");
        }

        return Math.Max(0, Math.Min(10, score));
    }

    private bool OnlyHappyPath(MethodDeclarationSyntax method)
    {
        // Look for patterns that indicate only happy path
        var statements = method.DescendantNodes().OfType<StatementSyntax>().ToList();

        // If only has one simple assertion, probably happy path
        var assertions = statements.OfType<ExpressionStatementSyntax>()
            .Where(s => s.Expression.ToString().Contains("Assert"))
            .ToList();

        return assertions.Count <= 1;
    }

    private int CountEdgeCases(MethodDeclarationSyntax method)
    {
        var text = method.ToString().ToLower();
        var edgeCases = 0;

        var edgePatterns = new[]
        {
            "null", "empty", "zero", "max", "min", "invalid", "negative",
            "boundary", "edge", "exception", "error"
        };

        foreach (var pattern in edgePatterns)
        {
            if (text.Contains(pattern))
                edgeCases++;
        }

        return edgeCases;
    }

    private int CountAssertions(MethodDeclarationSyntax method)
    {
        return method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Count(inv => inv.Expression.ToString().Contains("Assert"));
    }

    private bool TestsExceptions(MethodDeclarationSyntax method)
    {
        var text = method.ToString();
        return text.Contains("Throws") || text.Contains("Exception") ||
               text.Contains("ShouldThrow") || text.Contains("ExpectedException");
    }
}
