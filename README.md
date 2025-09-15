# Test Quality Auditor

Tool for auditing test quality in .NET projects, measuring **Completeness**, **Correctness** and **Maintainability**.

## 🎯 Problem it solves

In large companies it's common to have tests that technically cover code but don't add real value:
- Tests that only cover happy path
- Trivial assertions that never fail
- Mock abuse that doesn't validate real behavior
- Complex tests with multiple responsibilities

## 📊 Metrics

### Completeness (0-10)
- ✅ Edge case coverage (null, empty, boundary values)
- ✅ Exception validation  
- ✅ Multiple assertions
- ✅ Scenarios beyond happy path

### Correctness (0-10)
- ✅ Real assertions (not trivial like `Assert.True(true)`)
- ✅ Balanced mock usage
- ✅ Real behavior validation
- ✅ Tests that can fail

### Maintainability (0-10)
- ✅ Code without duplication
- ✅ Controlled complexity
- ✅ Single responsibilities
- ✅ Descriptive names
- ✅ Documentation

## 🚀 Usage

### Web UI (Recommended)
```bash
# Launch the web application
dotnet run --project TestQualityAuditor.Launcher
```
Then open http://localhost:5000 in your browser.

### CLI Tool
```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Analyze test project
dotnet run --project TestQualityAuditor.CLI -- --project "path/to/project.csproj"

# With output to file
dotnet run --project TestQualityAuditor.CLI -- --project "project.csproj" --output "report.json"
```

## 🖥️ Web UI Features

- **📁 Folder Selection**: Browse and select root directories containing test projects
- **🔍 Automatic Discovery**: Finds all test projects within the selected directory
- **📊 Interactive Results**: Sortable table with test quality metrics
- **📋 Copy Test Names**: Click the clipboard icon to copy test names for searching in code
- **💡 Recommendations**: Actionable suggestions for improving test quality
- **📱 Responsive Design**: Works on desktop and mobile devices

## 📋 Example output

```
🔍 Analyzing test quality...

📊 Analysis Results
Project: C:\Tests\MyProject.csproj
Analyzed: 2024-01-15 14:30:00

┌──────────────────┬──────────────┬──────────────┐
│ Metric           │ Score        │ Status       │
├──────────────────┼──────────────┼──────────────┤
│ Overall Score    │ 7.2/10       │ 🟡 Good      │
└──────────────────┴──────────────┴──────────────┘

📋 Analyzed Tests:
┌─────────────────────────┬────────────┬────────────┬──────────────┬─────────┐
│ Test                    │ Completeness│ Correctness│ Maintainability│ Overall │
├─────────────────────────┼────────────┼────────────┼──────────────┼─────────┤
│ UserService.CreateUser  │ 8.5        │ 7.0        │ 6.5          │ 7.3     │
│ UserService.UpdateUser  │ 6.0        │ 8.0        │ 7.0          │ 6.8     │
│ UserService.DeleteUser  │ 3.0        │ 4.0        │ 5.0          │ 3.8     │
└─────────────────────────┴────────────┴────────────┴──────────────┴─────────┘

💡 Recommendations:
• Improve edge case coverage and error scenarios
• Review trivial assertions and reduce mock abuse

⚠️ Tests that need attention:
• UserService.DeleteUser (3.8/10)
```

## 🔧 Installation

```bash
git clone <repo>
cd TestQualityAuditor
dotnet restore
dotnet build
```

## 🏗️ Architecture

```
TestQualityAuditor/
├── TestQualityAuditor.Core/           # Main analysis logic
│   ├── Models/                        # Data models
│   ├── Analyzers/                     # Specific analyzers
│   └── TestQualityAnalyzer.cs         # Main orchestrator
├── TestQualityAuditor.CLI/            # Console interface
├── TestQualityAuditor.Web/            # Web API and UI
│   ├── Controllers/                   # API endpoints
│   ├── Services/                      # Business logic
│   └── wwwroot/                       # Frontend (HTML/CSS/JS)
├── TestQualityAuditor.Launcher/       # One-click launcher
└── TestQualityAuditor.sln             # Solution
```

## 🎯 Use cases

### "Bad" tests it detects:
```csharp
[Test]
public void Test1()
{
    Assert.True(true); // ❌ Trivial assertion
}

[Test]
public void TestUser()
{
    var mock = new Mock<IUserService>();
    mock.Setup(x => x.GetUser(1)).Returns(new User());
    mock.Verify(x => x.GetUser(1), Times.Once()); // ❌ Only mocks, doesn't validate logic
}
```

### "Good" tests it values:
```csharp
[Test]
public void Should_CreateUser_When_ValidData()
{
    // Arrange
    var userService = new UserService();
    var userData = new UserData { Name = "Test", Email = "test@test.com" };
    
    // Act
    var result = userService.CreateUser(userData);
    
    // Assert
    Assert.IsNotNull(result);
    Assert.That(result.Name, Is.EqualTo("Test"));
    Assert.That(result.Email, Is.EqualTo("test@test.com"));
}

[Test]
public void Should_ThrowException_When_InvalidEmail()
{
    // Arrange
    var userService = new UserService();
    var invalidUserData = new UserData { Name = "Test", Email = "invalid-email" };
    
    // Act & Assert
    Assert.Throws<ValidationException>(() => userService.CreateUser(invalidUserData));
}
```

## 🚀 Roadmap

- [ ] CI/CD integration
- [ ] Web dashboard with charts
- [ ] Historical metrics comparison
- [ ] SonarQube integration
- [ ] Support for more test frameworks (xUnit, MSTest)
- [ ] Real coverage vs "fake" coverage analysis
- [ ] Detection of tests that never fail

## 🤝 Contributing

1. Fork the project
2. Create a branch for your feature (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is under the MIT License - see the [LICENSE](LICENSE) file for details.
