namespace TestQualityAuditor.Web.Services;

public class ProjectDiscoveryService
{
    public List<string> DiscoverTestProjects(string rootPath)
    {
        var testProjects = new List<string>();

        try
        {
            // Search for .csproj files in the directory and subdirectories
            var csprojFiles = Directory.GetFiles(rootPath, "*.csproj", SearchOption.AllDirectories);

            foreach (var projectFile in csprojFiles)
            {
                // Skip bin and obj directories
                if (projectFile.Contains("\\bin\\") || projectFile.Contains("\\obj\\"))
                    continue;

                // Check if it's likely a test project
                if (IsTestProject(projectFile))
                {
                    testProjects.Add(projectFile);
                }
            }
        }
        catch (Exception ex)
        {
            // Log error if needed
            Console.WriteLine($"Error discovering projects: {ex.Message}");
        }

        return testProjects;
    }

    private bool IsTestProject(string projectPath)
    {
        var fileName = Path.GetFileNameWithoutExtension(projectPath);

        // Common test project naming patterns
        var testPatterns = new[]
        {
            "Test", "Tests", "Testing", "Spec", "Specs", "Specification"
        };

        // Check if filename contains test patterns
        if (testPatterns.Any(pattern => fileName.Contains(pattern)))
            return true;

        // Check project file content for test-related packages
        try
        {
            var content = File.ReadAllText(projectPath);
            var testPackages = new[]
            {
                "Microsoft.NET.Test.Sdk",
                "NUnit",
                "xunit",
                "MSTest",
                "Moq",
                "FluentAssertions"
            };

            return testPackages.Any(package => content.Contains(package));
        }
        catch
        {
            return false;
        }
    }
}
