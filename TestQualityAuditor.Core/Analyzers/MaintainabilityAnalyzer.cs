using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestQualityAuditor.Core.Models;

namespace TestQualityAuditor.Core.Analyzers;

public class MaintainabilityAnalyzer
{
    public decimal AnalyzeMaintainability(MethodDeclarationSyntax testMethod, SemanticModel? semanticModel)
    {
        var score = 10m;
        var problems = new List<string>();

        // 1. Check code duplication
        if (HasCodeDuplication(testMethod))
        {
            score -= 2m;
            problems.Add("Code duplication detected");
        }

        // 2. Check test complexity
        var complexity = CalculateComplexity(testMethod);
        if (complexity > 10)
        {
            score -= 3m;
            problems.Add($"Test too complex (complexity: {complexity})");
        }

        // 3. Check multiple responsibilities
        if (HasMultipleResponsibilities(testMethod))
        {
            score -= 2m;
            problems.Add("Test with multiple responsibilities");
        }

        // 4. Check excessive setup
        if (HasExcessiveSetup(testMethod))
        {
            score -= 1m;
            problems.Add("Excessive test setup");
        }

        // 5. Check descriptive names
        if (!HasDescriptiveName(testMethod))
        {
            score -= 1m;
            problems.Add("Test name is not descriptive");
        }

        // 6. Check documentation
        if (!HasDocumentation(testMethod))
        {
            score -= 1m;
            problems.Add("Test without documentation");
        }

        return Math.Max(0, Math.Min(10, score));
    }

    private bool HasCodeDuplication(MethodDeclarationSyntax method)
    {
        var statements = method.DescendantNodes().OfType<StatementSyntax>().ToList();

        // Look for repeated patterns
        var textos = statements.Select(s => s.ToString().Trim()).ToList();

        return textos.GroupBy(t => t)
                    .Any(g => g.Count() > 1);
    }

    private int CalculateComplexity(MethodDeclarationSyntax method)
    {
        var complexity = 1; // Base complexity

        // Count control structures
        complexity += method.DescendantNodes().OfType<IfStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<ForStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<WhileStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<SwitchStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<TryStatementSyntax>().Count();

        // Count logical operators
        var texto = method.ToString();
        complexity += texto.Split(new[] { "&&", "||", "?" }, StringSplitOptions.None).Length - 1;

        return complexity;
    }

    private bool HasMultipleResponsibilities(MethodDeclarationSyntax method)
    {
        var assertions = method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Count(inv => inv.Expression.ToString().Contains("Assert"));

        // If has more than 5 assertions, probably tests multiple things
        return assertions > 5;
    }

    private bool HasExcessiveSetup(MethodDeclarationSyntax method)
    {
        var statements = method.DescendantNodes().OfType<StatementSyntax>().ToList();
        var setupStatements = statements.Count(s =>
            s.ToString().Contains("new ") ||
            s.ToString().Contains("Mock") ||
            s.ToString().Contains("Setup"));

        // If more than 50% of code is setup, it's excessive
        return setupStatements > statements.Count * 0.5;
    }

    private bool HasDescriptiveName(MethodDeclarationSyntax method)
    {
        var nombre = method.Identifier.ValueText;

        // Good names usually have "Should", "When", "Given", etc.
        var goodPatterns = new[] { "Should", "When", "Given", "If", "With" };

        if (goodPatterns.Any(p => nombre.Contains(p)))
            return true;

        // Bad names are usually generic
        var badPatterns = new[] { "Test1", "Test", "Method1", "TestMethod" };

        return !badPatterns.Any(p => nombre.Contains(p));
    }

    private bool HasDocumentation(MethodDeclarationSyntax method)
    {
        // Look for XML comments or comments before the method
        var trivia = method.GetLeadingTrivia();

        return trivia.Any(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                              t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));
    }
}
