# 🔍 Test Quality Auditor

Herramienta CLI para auditar la calidad de tests en proyectos .NET, midiendo **Completeness**, **Correctness** y **Maintainability**.

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

## 🚀 Cómo Usar

### Interfaz Web (Recomendado)
La herramienta se ejecuta desde su propia carpeta y permite especificar qué proyecto analizar mediante configuración.

```bash
# 1. Clona e inicia la herramienta
git clone https://github.com/tu-usuario/test-quality-auditor.git
cd test-quality-auditor

# 2. Restaura dependencias y ejecuta
dotnet restore
dotnet run --project TestQualityAuditor.Web

# 3. Abre tu navegador en http://localhost:5000

# 4. En la interfaz web, especifica la ruta de tu proyecto:
#    C:\ruta\a\tu\proyecto
#    /home/usuario/mi-proyecto
#    D:\Desarrollo\MiApp

# 5. Haz clic en "🔍 Analizar Tests"
```

**La aplicación ofrece dos modos:**

**📊 Análisis en Vivo:**
- 🔍 Escanea el directorio especificado buscando proyectos de test
- 📊 Los clasifica como Unit Tests o Integration Tests  
- 📋 Muestra resultados organizados en tabs separadas por tipo
- ⚡ Ejecuta el análisis completo y muestra métricas detalladas

**📄 Cargar JSON:**
- 📂 Carga reportes JSON generados previamente por la CLI
- 🎯 Mantiene las mismas tabs separadas por tipo de test
- ⚡ Visualización instantánea sin necesidad de re-analizar
- 🔄 Perfecto para compartir resultados o revisiones offline

### Herramienta CLI
```bash
# Analizar proyecto específico (.csproj)
dotnet run --project TestQualityAuditor.CLI -- --project "C:\MiApp\Tests\MyApp.Tests.csproj"

# Analizar todos los tests en un directorio
dotnet run --project TestQualityAuditor.CLI -- --project "C:\MiApp\"

# Con salida a archivo JSON
dotnet run --project TestQualityAuditor.CLI -- --project "C:\MiApp\" --output "report.json"

# Ver ayuda
dotnet run --project TestQualityAuditor.CLI -- --help
```

#### 🎯 Ejemplos de Uso CLI

**Analizar proyecto específico:**
```bash
dotnet run --project TestQualityAuditor.CLI -- --project "D:\MyApp\Tests\MyApp.UnitTests.csproj"
```

**Analizar directorio completo (recomendado):**
```bash
dotnet run --project TestQualityAuditor.CLI -- --project "D:\MyApp\"
```

**Generar reporte JSON:**
```bash
dotnet run --project TestQualityAuditor.CLI -- --project "D:\MyApp\" --output "test-quality-report.json"
```

## 🖥️ Características de la Interfaz Web

- **🔍 Detección Automática**: Encuentra todos los proyectos de test en el directorio actual
- **📊 Clasificación Inteligente**: Separa Unit Tests e Integration Tests automáticamente
- **📋 Navegación por Tabs**: Resultados organizados en pestañas separadas por tipo
- **📈 Métricas Interactivas**: Tabla ordenable con métricas de calidad de tests
- **📋 Copiar Nombres**: Haz clic en el icono del portapapeles para copiar nombres de tests
- **💡 Recomendaciones**: Sugerencias accionables para mejorar la calidad de tests
- **📱 Diseño Responsive**: Funciona en desktop y dispositivos móviles

### Tipos de Test Detectados Automáticamente

**Integration Tests** - Proyectos que contienen:
- `integration`, `integrationtest`, `integrationtests`
- `e2e`, `endtoend`, `functional`, `acceptance`

**Unit Tests** - Proyectos que contienen:
- `unit`, `unittest`, `unittests`, `test`, `tests`

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

## 🔧 Instalación y Configuración

### Instalación Única
```bash
# 1. Clona el repositorio en tu ubicación preferida
git clone https://github.com/tu-usuario/test-quality-auditor.git
cd test-quality-auditor

# 2. Restaura dependencias
dotnet restore
dotnet build

# 3. Ya puedes usar la herramienta para analizar cualquier proyecto
dotnet run --project TestQualityAuditor.Web

# 4. La herramienta se ejecuta en http://localhost:5000
#    Desde ahí puedes especificar cualquier ruta de proyecto a analizar
```

### 🎯 Ventajas de este Enfoque
- ✅ **Una sola instalación** - La herramienta vive en su propia carpeta
- ✅ **Analiza cualquier proyecto** - Solo especifica la ruta en la interfaz
- ✅ **No contamina proyectos** - No necesitas clonar nada en tus proyectos
- ✅ **Reutilizable** - Analiza múltiples proyectos desde la misma instalación

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

## ⚡ Flujo de Trabajo Típico

### 🚀 Comando Unificado (Súper Fácil)
```bash
# ¡TODO EN UN SOLO COMANDO! 🎉
dotnet run --project TestQualityAuditor.Launcher -- "C:\MiApp\"

# Esto automáticamente:
# 1. 🔍 Ejecuta el análisis CLI
# 2. 📄 Genera el reporte.json  
# 3. 🌐 Levanta la interfaz web
# 4. 🎯 Abre el navegador con el reporte cargado
```

### Flujo Manual (Si prefieres paso a paso)
```bash
# 1. Generar reporte con CLI
dotnet run --project TestQualityAuditor.CLI -- --project "C:\MiApp\" --output "reporte.json"

# 2. Iniciar interfaz web (desde la misma carpeta)
dotnet run --project TestQualityAuditor.Web

# 3. Abrir http://localhost:5000 → ¡Se carga automáticamente!
```

### 🎯 Comportamiento Automático

**✅ Si existe `reporte.json`:**
- Carga automáticamente al abrir la página
- Muestra resultados organizados en tabs:
  - 📊 **All Tests** - Vista general
  - 🧪 **Unit Tests** - Solo tests unitarios  
  - 🔗 **Integration Tests** - Solo tests de integración

**❌ Si NO existe `reporte.json`:**
- Muestra mensaje indicando que no hay reporte
- Proporciona instrucciones para generar uno

### 💡 Ejemplos de Rutas Válidas

**Windows:**
```
C:\Desarrollo\MiApp
D:\Proyectos\Backend\Tests
C:\Users\Usuario\source\repos\MiProyecto
```

**Linux/Mac:**
```
/home/usuario/proyectos/mi-app
/Users/usuario/Development/backend
/opt/proyectos/tests
```

## 🚀 Roadmap

- [x] **Detección automática de proyectos de test**
- [x] **Clasificación Unit vs Integration tests**
- [x] **Interfaz con tabs separadas por tipo**
- [ ] CI/CD integration
- [ ] Exportar reportes a PDF/Excel
- [ ] Métricas históricas y comparación
- [ ] Integración con SonarQube
- [ ] Soporte para más frameworks (xUnit, MSTest)
- [ ] Análisis de cobertura real vs "falsa"
- [ ] Detección de tests que nunca fallan

## 🤝 Contributing

1. Fork the project
2. Create a branch for your feature (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is under the MIT License - see the [LICENSE](LICENSE) file for details.

## Example: dotnet run --project TestQualityAuditor.Launcher -- "D:\Work\tam\SampleTestProject"
