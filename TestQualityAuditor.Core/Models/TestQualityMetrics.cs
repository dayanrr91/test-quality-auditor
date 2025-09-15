namespace TestQualityAuditor.Core.Models;

public class TestQualityMetrics
{
    public decimal Completeness { get; set; } // 0-10
    public decimal Correctness { get; set; } // 0-10
    public decimal Maintainability { get; set; } // 0-10
    public decimal OverallScore { get; set; } // 0-10
    public List<string> Problems { get; set; } = new();
    public Dictionary<string, object> Details { get; set; } = new();
}

public class TestAnalysisResult
{
    public string TestMethodName { get; set; } = string.Empty;
    public string TestClassName { get; set; } = string.Empty;
    public TestQualityMetrics Metrics { get; set; } = new();
    public List<TestIssue> Issues { get; set; } = new();
}

public class TestIssue
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public int Line { get; set; }
}

public class ProjectAnalysisResult
{
    public string ProjectPath { get; set; } = string.Empty;
    public decimal OverallCoverage { get; set; }
    public decimal AverageScore { get; set; }
    public List<TestAnalysisResult> TestResults { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime AnalyzedAt { get; set; } = DateTime.Now;
}
