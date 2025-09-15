# Test Quality Auditor - Live Demo

This is the static site version of Test Quality Auditor hosted on GitHub Pages.

## 🚀 Features

- **Interactive Web Interface**: Browse and analyze test projects
- **Automatic Project Discovery**: Finds all test projects within directories
- **Detailed Quality Metrics**: Completeness, Correctness, and Maintainability scores
- **Smart Recommendations**: Actionable suggestions for improving test quality

## 📊 Metrics

### Completeness (0-10)
- Edge case coverage
- Exception validation
- Multiple assertions
- Scenarios beyond happy path

### Correctness (0-10)
- Real assertions (not trivial)
- Balanced mock usage
- Real behavior validation
- Tests that can fail

### Maintainability (0-10)
- Code without duplication
- Controlled complexity
- Single responsibilities
- Descriptive names

## 🔧 Usage

1. **Clone the repository**
2. **Run the application locally**:
   ```bash
   dotnet run --project TestQualityAuditor.Launcher
   ```
3. **Open http://localhost:5000** in your browser
4. **Select a folder** containing test projects
5. **View the analysis results**

## 📁 Project Structure

```
TestQualityAuditor/
├── TestQualityAuditor.Core/           # Main analysis logic
├── TestQualityAuditor.CLI/            # Console interface
├── TestQualityAuditor.Web/            # Web API and UI
├── TestQualityAuditor.Launcher/       # One-click launcher
└── static-site/                       # GitHub Pages static site
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.
