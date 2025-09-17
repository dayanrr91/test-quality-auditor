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
            var targetPath = string.IsNullOrEmpty(request.RootPath)
                ? Directory.GetCurrentDirectory()
                : request.RootPath;

            if (!Directory.Exists(targetPath))
            {
                return BadRequest($"Directory does not exist: {targetPath}");
            }

            var testProjects = _projectDiscovery.DiscoverTestProjects(targetPath);

            var response = new DiscoverProjectsResponse
            {
                RootPath = targetPath,
                TestProjects = testProjects.Select(p => new ProjectInfo
                {
                    Path = p,
                    Name = Path.GetFileNameWithoutExtension(p),
                    RelativePath = Path.GetRelativePath(targetPath, p),
                    TestType = _projectDiscovery.DetermineTestType(p)
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

    [HttpGet("load-report")]
    public IActionResult LoadReport()
    {
        try
        {
            var currentDir = Directory.GetCurrentDirectory();
            var parentDir = Directory.GetParent(currentDir)?.FullName ?? currentDir;

            _logger.LogInformation($"Current directory: {currentDir}");
            _logger.LogInformation($"Parent directory: {parentDir}");

            // Buscar en múltiples ubicaciones posibles
            var possiblePaths = new[]
            {
                Path.Combine(parentDir, "reporte.json"),           // Directorio padre
                Path.Combine(currentDir, "reporte.json"),          // Directorio actual
                Path.Combine(currentDir, "..", "reporte.json"),    // Relativo al padre
                "reporte.json"                                     // Directorio de trabajo
            };

            string? foundPath = null;
            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                _logger.LogInformation($"Checking path: {fullPath}");

                if (System.IO.File.Exists(fullPath))
                {
                    foundPath = fullPath;
                    _logger.LogInformation($"Found report at: {foundPath}");
                    break;
                }
            }

            if (foundPath == null)
            {
                _logger.LogWarning("Report file not found in any of the expected locations");
                return NotFound(new
                {
                    message = "No se encontró el archivo reporte.json",
                    searchedPaths = possiblePaths.Select(Path.GetFullPath).ToArray(),
                    currentDirectory = currentDir
                });
            }

            var jsonContent = System.IO.File.ReadAllText(foundPath);
            var reportData = System.Text.Json.JsonSerializer.Deserialize<object>(jsonContent);

            return Ok(reportData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading report");
            return StatusCode(500, new { message = "Error al cargar el reporte: " + ex.Message });
        }
    }

    [HttpPost("analyze-all")]
    public async Task<IActionResult> AnalyzeAllProjects([FromBody] AnalyzeAllProjectsRequest request)
    {
        try
        {
            var targetPath = string.IsNullOrEmpty(request.RootPath)
                ? Directory.GetCurrentDirectory()
                : request.RootPath;

            if (!Directory.Exists(targetPath))
            {
                return BadRequest($"Directory does not exist: {targetPath}");
            }

            var testProjects = _projectDiscovery.DiscoverTestProjects(targetPath);
            var results = new List<ProjectAnalysisResult>();

            foreach (var projectPath in testProjects)
            {
                try
                {
                    var result = await _analyzer.AnalyzeProject(projectPath);
                    result.TestType = _projectDiscovery.DetermineTestType(projectPath);
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
    public string TestType { get; set; } = string.Empty;
}

public class AnalyzeProjectRequest
{
    public string ProjectPath { get; set; } = string.Empty;
}

public class AnalyzeAllProjectsRequest
{
    public string RootPath { get; set; } = string.Empty;
}
