class TestQualityAuditor {
  constructor() {
    this.discoveredProjects = [];
    this.analysisResults = [];
    this.initializeEventListeners();
    this.loadReportAutomatically();
  }

  initializeEventListeners() {
    // Results tabs
    const tabButtons = document.querySelectorAll(".tab-button");
    tabButtons.forEach((button) => {
      button.addEventListener("click", (e) => {
        this.switchTab(e.target.dataset.tab);
      });
    });
  }

  async loadReportAutomatically() {
    console.log("🔍 Iniciando carga automática del reporte...");
    this.showLoading(true);
    this.hideError();

    try {
      console.log("📡 Haciendo petición a /api/analysis/load-report");
      const response = await fetch("/api/analysis/load-report");
      console.log(
        "📡 Respuesta recibida:",
        response.status,
        response.statusText
      );

      if (response.status === 404) {
        console.log("❌ No hay reporte disponible (404)");
        this.showNoReportMessage();
        return;
      }

      if (!response.ok) {
        const errorText = await response.text();
        console.error("❌ Error en respuesta:", response.status, errorText);
        throw new Error(
          `HTTP error! status: ${response.status} - ${errorText}`
        );
      }

      console.log("📄 Parseando JSON...");
      const reportData = await response.json();
      console.log("✅ JSON parseado correctamente:", reportData);

      console.log("🔄 Cargando resultados...");
      this.loadJsonResults(reportData);

      // Actualizar mensaje de estado
      const statusMessage = document.getElementById("statusMessage");
      statusMessage.textContent = "✅ Reporte cargado automáticamente";
      statusMessage.style.color = "#28a745";
      console.log("✅ Reporte cargado exitosamente!");
    } catch (error) {
      console.error("❌ Error loading report:", error);
      this.showError("Error cargando el reporte: " + error.message);
    } finally {
      this.showLoading(false);
    }
  }

  showNoReportMessage() {
    const noReportMessage = document.getElementById("noReportMessage");
    const statusMessage = document.getElementById("statusMessage");

    statusMessage.textContent = "❌ No se encontró reporte.json";
    statusMessage.style.color = "#dc3545";
    noReportMessage.classList.remove("hidden");
  }

  loadJsonResults(jsonData) {
    console.log("🔄 Procesando datos JSON:", jsonData);
    this.hideError();
    this.hideResults();

    try {
      // Check if it's a single project or array of projects
      let results = Array.isArray(jsonData) ? jsonData : [jsonData];
      console.log("📊 Resultados procesados:", results);

      // Validate that results have the expected structure
      results.forEach((result, index) => {
        console.log(`📋 Proyecto ${index} COMPLETO:`, result);
        console.log(`📋 Proyecto ${index} DETALLES:`, {
          projectPath: result.projectPath,
          testType: result.testType,
          testResults: result.testResults
            ? result.testResults.length
            : "undefined",
          allKeys: Object.keys(result),
        });

        // Log all properties to understand the structure
        console.log("🔍 TODAS LAS PROPIEDADES:");
        Object.keys(result).forEach((key) => {
          console.log(`  ${key}:`, result[key]);
        });
      });

      // Show projects list for JSON data
      this.displayJsonProjects(results);

      this.analysisResults = results;
      this.displayResults(results);
    } catch (error) {
      console.error("❌ Error procesando JSON:", error);
      this.showError("Error procesando el reporte: " + error.message);
    }
  }

  displayJsonProjects(results) {
    const projectsList = document.getElementById("projectsList");
    const projectsContainer = document.getElementById("projectsContainer");

    if (results.length === 0) {
      projectsContainer.innerHTML =
        "<p>No se encontraron datos en el archivo JSON.</p>";
    } else {
      projectsContainer.innerHTML = `
        <div style="margin-bottom: 15px; padding: 10px; background: #e8f5e8; border-radius: 5px;">
          <strong>📄 Cargado desde archivo JSON</strong> - ${
            results.length
          } proyecto(s)
        </div>
        ${results
          .map((project) => {
            const projectName = project.projectPath
              ? project.projectPath.split("\\").pop().replace(".csproj", "")
              : "Proyecto desconocido";
            const testType = project.testType || "Unit";

            return `
                <div class="project-item">
                    <div class="project-info">
                        <div class="project-name">
                            ${projectName}
                            <span class="test-type-badge test-type-${testType.toLowerCase()}">
                                ${testType}
                            </span>
                        </div>
                        <div class="project-path">${
                          project.projectPath || "Ruta no disponible"
                        }</div>
                    </div>
                </div>
              `;
          })
          .join("")}
      `;
    }

    projectsList.classList.remove("hidden");
  }

  switchTab(tabName) {
    // Remove active class from all tabs and buttons
    document
      .querySelectorAll(".tab-button")
      .forEach((btn) => btn.classList.remove("active"));
    document
      .querySelectorAll(".tab-content")
      .forEach((content) => content.classList.remove("active"));

    // Add active class to selected tab and button
    document.querySelector(`[data-tab="${tabName}"]`).classList.add("active");
    document.getElementById(`${tabName}TestsTab`).classList.add("active");
  }

  displayResults(results) {
    console.log("📊 Mostrando resultados:", results);
    const resultsDiv = document.getElementById("results");

    try {
      // Validate results
      if (!results || !Array.isArray(results)) {
        throw new Error("Los resultados no son válidos o no es un array");
      }

      // Separate results by test type (handle both camelCase and PascalCase)
      const unitResults = results.filter(
        (r) => (r.testType || r.TestType) === "Unit"
      );
      const integrationResults = results.filter(
        (r) => (r.testType || r.TestType) === "Integration"
      );

      console.log("🧪 Unit tests:", unitResults.length);
      console.log("🔗 Integration tests:", integrationResults.length);

      // Display all results
      this.displayResultsInContainer(results, "allResultsContainer");

      // Display unit test results
      this.displayResultsInContainer(unitResults, "unitResultsContainer");

      // Display integration test results
      this.displayResultsInContainer(
        integrationResults,
        "integrationResultsContainer"
      );

      resultsDiv.classList.remove("hidden");
    } catch (error) {
      console.error("❌ Error mostrando resultados:", error);
      this.showError("Error mostrando resultados: " + error.message);
    }
  }

  displayResultsInContainer(results, containerId) {
    const container = document.getElementById(containerId);

    if (!results || results.length === 0) {
      container.innerHTML =
        "<p>No analysis results available for this test type.</p>";
      return;
    }

    let html = "";

    try {
      // Overall summary for this category
      const totalTests = results.reduce((sum, project) => {
        const testResults = project.testResults || project.TestResults || [];
        const projectPath =
          project.projectPath || project.ProjectPath || "Unknown";
        console.log(`📊 Proyecto ${projectPath}: ${testResults.length} tests`);
        return sum + testResults.length;
      }, 0);
      const averageScore =
        results.length > 0
          ? results.reduce((sum, project) => {
              const score = project.averageScore || project.AverageScore || 0;
              return sum + score;
            }, 0) / results.length
          : 0;

      html += `
      <div class="metrics-grid">
          <div class="metric-card">
              <div class="metric-value">${results.length}</div>
              <div class="metric-label">Projects Analyzed</div>
          </div>
          <div class="metric-card">
              <div class="metric-value">${totalTests}</div>
              <div class="metric-label">Total Tests</div>
          </div>
          <div class="metric-card">
              <div class="metric-value">${averageScore.toFixed(1)}/10</div>
              <div class="metric-label">Average Score</div>
          </div>
      </div>
    `;

      // Individual project results
      results.forEach((project) => {
        html += this.createProjectResultsHtml(project);
      });

      container.innerHTML = html;
    } catch (error) {
      console.error(
        `❌ Error en displayResultsInContainer (${containerId}):`,
        error
      );
      container.innerHTML = `<p>Error mostrando resultados: ${error.message}</p>`;
    }
  }

  createProjectResultsHtml(project) {
    console.log("🏗️ Creando HTML para proyecto:", project);

    // Handle cases where projectPath might be undefined or have different property names
    const projectPath =
      project.projectPath || project.ProjectPath || "Unknown path";
    const testType = project.testType || project.TestType || "Unit";
    const testResults = project.testResults || project.TestResults || [];
    const averageScore = project.averageScore || project.AverageScore || 0;

    const projectName =
      projectPath !== "Unknown path"
        ? projectPath
            .split(/[\\\/]/)
            .pop()
            .replace(".csproj", "")
        : "Proyecto desconocido";

    const testTypeBadge = testType
      ? `<span class="test-type-badge test-type-${testType.toLowerCase()}">${testType}</span>`
      : "";

    let html = `
            <div class="card">
                <h3>📁 ${projectName} ${testTypeBadge}</h3>
                <p><strong>Path:</strong> ${projectPath}</p>
                
                <div class="metrics-grid">
                    <div class="metric-card">
                        <div class="metric-value">${averageScore.toFixed(
                          1
                        )}/10</div>
                        <div class="metric-label">Overall Score</div>
                    </div>
                    <div class="metric-card">
                        <div class="metric-value">${testResults.length}</div>
                        <div class="metric-label">Tests Found</div>
                    </div>
                </div>
        `;

    if (testResults.length > 0) {
      html += `
                 <div class="table-container">
                     <table class="tests-table">
                         <thead>
                             <tr>
                                 <th style="width: 40%;">Test Method</th>
                                 <th style="width: 15%;">Completeness</th>
                                 <th style="width: 15%;">Correctness</th>
                                 <th style="width: 15%;">Maintainability</th>
                                 <th style="width: 15%;">Overall</th>
                             </tr>
                         </thead>
                         <tbody>
             `;

      testResults
        .sort((a, b) => {
          // Handle both camelCase and PascalCase property names
          const scoreA =
            (a.metrics && a.metrics.overallScore) ||
            (a.Metrics && a.Metrics.OverallScore) ||
            0;
          const scoreB =
            (b.metrics && b.metrics.overallScore) ||
            (b.Metrics && b.Metrics.OverallScore) ||
            0;
          return scoreB - scoreA;
        })
        .forEach((test) => {
          const testClassName =
            test.testClassName || test.TestClassName || "Unknown";
          const testMethodName =
            test.testMethodName || test.TestMethodName || "Unknown";
          const fullTestName = `${testClassName}.${testMethodName}`;
          const displayName = this.truncateText(fullTestName, 50);

          // Handle both camelCase and PascalCase metrics
          const metrics = test.metrics || test.Metrics || {};
          const completeness =
            metrics.completeness || metrics.Completeness || 0;
          const correctness = metrics.correctness || metrics.Correctness || 0;
          const maintainability =
            metrics.maintainability || metrics.Maintainability || 0;
          const overallScore =
            metrics.overallScore || metrics.OverallScore || 0;

          html += `
                         <tr>
                             <td>
                                 <div class="test-name" data-full-name="${this.escapeHtml(
                                   fullTestName
                                 )}" title="${this.escapeHtml(fullTestName)}">
                                     <span class="copy-icon" onclick="app.copyToClipboard('${this.escapeHtml(
                                       fullTestName
                                     )}')" title="Copy test name to clipboard">📋</span>
                                     ${this.escapeHtml(displayName)}
                                 </div>
                             </td>
                             <td><span class="score ${this.getScoreClass(
                               completeness
                             )}">${completeness.toFixed(1)}</span></td>
                             <td><span class="score ${this.getScoreClass(
                               correctness
                             )}">${correctness.toFixed(1)}</span></td>
                             <td><span class="score ${this.getScoreClass(
                               maintainability
                             )}">${maintainability.toFixed(1)}</span></td>
                             <td><span class="score ${this.getScoreClass(
                               overallScore
                             )}">${overallScore.toFixed(1)}</span></td>
                         </tr>
                     `;
        });

      html += "</tbody></table></div>";
    }

    const recommendations =
      project.recommendations || project.Recommendations || [];
    if (recommendations.length > 0) {
      html += `
                <div class="recommendations">
                    <h3>💡 Recommendations</h3>
                    <ul>
                        ${recommendations
                          .map((rec) => `<li>${rec}</li>`)
                          .join("")}
                    </ul>
                </div>
            `;
    }

    html += "</div>";
    return html;
  }

  getScoreClass(score) {
    if (score >= 8) return "excellent";
    if (score >= 6) return "good";
    if (score >= 4) return "fair";
    return "poor";
  }

  showLoading(show) {
    const loading = document.getElementById("loading");

    if (show) {
      loading.classList.remove("hidden");
    } else {
      loading.classList.add("hidden");
    }
  }

  showError(message) {
    const errorDiv = document.getElementById("error");
    errorDiv.textContent = message;
    errorDiv.classList.remove("hidden");
  }

  hideError() {
    const errorDiv = document.getElementById("error");
    errorDiv.classList.add("hidden");
  }

  hideResults() {
    const resultsDiv = document.getElementById("results");
    resultsDiv.classList.add("hidden");
  }

  truncateText(text, maxLength) {
    if (text.length <= maxLength) {
      return text;
    }
    return text.substring(0, maxLength) + "...";
  }

  escapeHtml(text) {
    const div = document.createElement("div");
    div.textContent = text;
    return div.innerHTML;
  }

  async copyToClipboard(text) {
    try {
      await navigator.clipboard.writeText(text);
      this.showCopyNotification();
    } catch (err) {
      // Fallback for older browsers
      const textArea = document.createElement("textarea");
      textArea.value = text;
      document.body.appendChild(textArea);
      textArea.select();
      document.execCommand("copy");
      document.body.removeChild(textArea);
      this.showCopyNotification();
    }
  }

  showCopyNotification() {
    // Create a temporary notification
    const notification = document.createElement("div");
    notification.textContent = "Test name copied to clipboard!";
    notification.style.cssText = `
      position: fixed;
      top: 20px;
      right: 20px;
      background: #28a745;
      color: white;
      padding: 12px 20px;
      border-radius: 5px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.2);
      z-index: 10000;
      font-size: 0.9rem;
      transition: opacity 0.3s;
    `;

    document.body.appendChild(notification);

    // Remove after 2 seconds
    setTimeout(() => {
      notification.style.opacity = "0";
      setTimeout(() => {
        document.body.removeChild(notification);
      }, 300);
    }, 2000);
  }
}

// Initialize the application when the page loads
let app;
document.addEventListener("DOMContentLoaded", () => {
  app = new TestQualityAuditor();
});
