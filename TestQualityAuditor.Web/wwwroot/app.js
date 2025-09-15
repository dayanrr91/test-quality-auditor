class TestQualityAuditor {
  constructor() {
    this.selectedPath = null;
    this.discoveredProjects = [];
    this.initializeEventListeners();
  }

  initializeEventListeners() {
    const selectFolderBtn = document.getElementById("selectFolderBtn");
    const folderPathInput = document.getElementById("folderPathInput");
    const analyzeBtn = document.getElementById("analyzeBtn");

    selectFolderBtn.addEventListener("click", () => {
      this.handleFolderSelection();
    });

    folderPathInput.addEventListener("keypress", (event) => {
      if (event.key === "Enter") {
        this.handleFolderSelection();
      }
    });

    analyzeBtn.addEventListener("click", () => {
      this.analyzeAllProjects();
    });
  }

  handleFolderSelection() {
    const folderPathInput = document.getElementById("folderPathInput");
    const fullPath = folderPathInput.value.trim();

    if (!fullPath) {
      alert("Please enter a folder path");
      return;
    }

    this.selectedPath = fullPath;

    // Show selected path
    const selectedPathDiv = document.getElementById("selectedPath");
    selectedPathDiv.textContent = `Selected: ${fullPath}`;
    selectedPathDiv.classList.remove("hidden");

    // Show analyze button
    const analyzeBtn = document.getElementById("analyzeBtn");
    analyzeBtn.classList.remove("hidden");

    // Discover projects
    this.discoverProjects(fullPath);
  }

  async discoverProjects(rootPath) {
    this.showLoading(true);
    this.hideError();

    try {
      const response = await fetch("/api/analysis/discover-projects", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ rootPath: rootPath }),
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = await response.json();
      this.discoveredProjects = data.testProjects;
      this.displayDiscoveredProjects(data);
    } catch (error) {
      console.error("Error discovering projects:", error);
      this.showError("Error discovering test projects: " + error.message);
    } finally {
      this.showLoading(false);
    }
  }

  displayDiscoveredProjects(data) {
    const projectsList = document.getElementById("projectsList");
    const projectsContainer = document.getElementById("projectsContainer");

    if (data.testProjects.length === 0) {
      projectsContainer.innerHTML =
        "<p>No test projects found in the selected directory.</p>";
    } else {
      projectsContainer.innerHTML = data.testProjects
        .map(
          (project) => `
                <div class="project-item">
                    <div class="project-info">
                        <div class="project-name">${project.name}</div>
                        <div class="project-path">${project.relativePath}</div>
                    </div>
                </div>
            `
        )
        .join("");
    }

    projectsList.classList.remove("hidden");
  }

  async analyzeAllProjects() {
    if (!this.selectedPath) return;

    this.showLoading(true);
    this.hideError();
    this.hideResults();

    try {
      const response = await fetch("/api/analysis/analyze-all", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ rootPath: this.selectedPath }),
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const results = await response.json();
      this.displayResults(results);
    } catch (error) {
      console.error("Error analyzing projects:", error);
      this.showError("Error analyzing projects: " + error.message);
    } finally {
      this.showLoading(false);
    }
  }

  displayResults(results) {
    const resultsDiv = document.getElementById("results");
    const resultsContainer = document.getElementById("resultsContainer");

    if (results.length === 0) {
      resultsContainer.innerHTML = "<p>No analysis results available.</p>";
    } else {
      let html = "";

      // Overall summary
      const totalTests = results.reduce(
        (sum, project) => sum + project.testResults.length,
        0
      );
      const averageScore =
        results.reduce((sum, project) => sum + project.averageScore, 0) /
        results.length;

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
                        <div class="metric-value">${averageScore.toFixed(
                          1
                        )}/10</div>
                        <div class="metric-label">Average Score</div>
                    </div>
                </div>
            `;

      // Individual project results
      results.forEach((project) => {
        html += this.createProjectResultsHtml(project);
      });

      resultsContainer.innerHTML = html;
    }

    resultsDiv.classList.remove("hidden");
  }

  createProjectResultsHtml(project) {
    const projectName = project.projectPath
      .split("\\")
      .pop()
      .replace(".csproj", "");

    let html = `
            <div class="card">
                <h3>📁 ${projectName}</h3>
                <p><strong>Path:</strong> ${project.projectPath}</p>
                
                <div class="metrics-grid">
                    <div class="metric-card">
                        <div class="metric-value">${project.averageScore.toFixed(
                          1
                        )}/10</div>
                        <div class="metric-label">Overall Score</div>
                    </div>
                    <div class="metric-card">
                        <div class="metric-value">${
                          project.testResults.length
                        }</div>
                        <div class="metric-label">Tests Found</div>
                    </div>
                </div>
        `;

    if (project.testResults.length > 0) {
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

      project.testResults
        .sort((a, b) => b.metrics.overallScore - a.metrics.overallScore)
        .forEach((test) => {
          const fullTestName = `${test.testClassName}.${test.testMethodName}`;
          const displayName = this.truncateText(fullTestName, 50);

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
                               test.metrics.completeness
                             )}">${test.metrics.completeness.toFixed(
            1
          )}</span></td>
                             <td><span class="score ${this.getScoreClass(
                               test.metrics.correctness
                             )}">${test.metrics.correctness.toFixed(
            1
          )}</span></td>
                             <td><span class="score ${this.getScoreClass(
                               test.metrics.maintainability
                             )}">${test.metrics.maintainability.toFixed(
            1
          )}</span></td>
                             <td><span class="score ${this.getScoreClass(
                               test.metrics.overallScore
                             )}">${test.metrics.overallScore.toFixed(
            1
          )}</span></td>
                         </tr>
                     `;
        });

      html += "</tbody></table></div>";
    }

    if (project.recommendations && project.recommendations.length > 0) {
      html += `
                <div class="recommendations">
                    <h3>💡 Recommendations</h3>
                    <ul>
                        ${project.recommendations
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
    const analyzeBtn = document.getElementById("analyzeBtn");

    if (show) {
      loading.classList.remove("hidden");
      analyzeBtn.disabled = true;
    } else {
      loading.classList.add("hidden");
      analyzeBtn.disabled = false;
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
