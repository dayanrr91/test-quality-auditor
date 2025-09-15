using Microsoft.AspNetCore.Mvc;
using TestQualityAuditor.Core;
using TestQualityAuditor.Core.Models;
using TestQualityAuditor.Web.Services;

namespace TestQualityAuditor.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly TestQualityAnalyzer _analyzer;
    private readonly ProjectDiscoveryService _projectDiscovery;
    private readonly ILogger<AnalysisController> _logger;

    public AnalysisController(
        TestQualityAnalyzer analyzer,
        ProjectDiscoveryService projectDiscovery,
        ILogger<AnalysisController> logger)
    {
        _analyzer = analyzer;
        _projectDiscovery = projectDiscovery;
        _logger = logger;
    }

    [HttpPost("discover-projects")]
    public IActionResult DiscoverProjects([FromBody] DiscoverProjectsRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.RootPath) || !Directory.Exists(request.RootPath))
            {
                return BadRequest("Invalid root path provided");
            }

            var testProjects = _projectDiscovery.DiscoverTestProjects(request.RootPath);

            var response = new DiscoverProjectsResponse
            {
                RootPath = request.RootPath,
                TestProjects = testProjects.Select(p => new ProjectInfo
                {
                    Path = p,
                    Name = Path.GetFileNameWithoutExtension(p),
                    RelativePath = Path.GetRelativePath(request.RootPath, p)
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering projects");
            return StatusCode(500, "Error discovering test projects");
        }
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeProject([FromBody] AnalyzeProjectRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ProjectPath) || !System.IO.File.Exists(request.ProjectPath))
            {
                return BadRequest("Invalid project path provided");
            }

            var result = await _analyzer.AnalyzeProject(request.ProjectPath);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing project {ProjectPath}", request.ProjectPath);
            return StatusCode(500, $"Error analyzing project: {ex.Message}");
        }
    }

    [HttpPost("analyze-all")]
    public async Task<IActionResult> AnalyzeAllProjects([FromBody] AnalyzeAllProjectsRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.RootPath) || !Directory.Exists(request.RootPath))
            {
                return BadRequest("Invalid root path provided");
            }

            var testProjects = _projectDiscovery.DiscoverTestProjects(request.RootPath);
            var results = new List<ProjectAnalysisResult>();

            foreach (var projectPath in testProjects)
            {
                try
                {
                    var result = await _analyzer.AnalyzeProject(projectPath);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error analyzing project {ProjectPath}", projectPath);
                    // Continue with other projects
                }
            }

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing all projects");
            return StatusCode(500, "Error analyzing projects");
        }
    }
}

public class DiscoverProjectsRequest
{
    public string RootPath { get; set; } = string.Empty;
}

public class DiscoverProjectsResponse
{
    public string RootPath { get; set; } = string.Empty;
    public List<ProjectInfo> TestProjects { get; set; } = new();
}

public class ProjectInfo
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
}

public class AnalyzeProjectRequest
{
    public string ProjectPath { get; set; } = string.Empty;
}

public class AnalyzeAllProjectsRequest
{
    public string RootPath { get; set; } = string.Empty;
}
