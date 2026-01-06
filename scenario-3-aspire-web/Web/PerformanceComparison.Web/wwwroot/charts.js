// Chart.js initialization and utilities
let charts = {};

// Load Chart.js library
function loadChartJs() {
    return new Promise((resolve, reject) => {
        if (typeof Chart !== 'undefined') {
            resolve();
            return;
        }

        const script = document.createElement('script');
        script.src = 'https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js';
        // TODO: Add SRI hash after verifying against official CDN. Use https://www.srihash.org/ to generate.
        // script.integrity = 'sha384-...';
        // script.crossOrigin = 'anonymous';
        script.async = true;
        script.onload = () => resolve();
        script.onerror = () => reject(new Error('Failed to load Chart.js'));
        document.head.appendChild(script);
    });
}

// Initialize library on first use
loadChartJs().catch(err => console.error('Chart.js loading error:', err));

// Detect anomalies using 2 standard deviations
function detectAnomalies(data) {
    if (!data || data.length < 3) return [];
    
    const mean = data.reduce((sum, val) => sum + val, 0) / data.length;
    const variance = data.reduce((sum, val) => sum + Math.pow(val - mean, 2), 0) / data.length;
    const stdDev = Math.sqrt(variance);
    const threshold = 2 * stdDev;
    
    return data.map((val, idx) => Math.abs(val - mean) > threshold ? idx : -1).filter(idx => idx >= 0);
}

// Render trends chart (performance over iterations) with anomaly highlighting
window.renderTrendsChart = async function (labels, dotnetTimes, pythonTimes) {
    await loadChartJs();

    const ctx = document.getElementById('trendsChart');
    if (!ctx) return;

    // Destroy existing chart if any
    if (charts.trends) {
        charts.trends.destroy();
    }

    // Detect anomalies
    const dotnetAnomalies = detectAnomalies(dotnetTimes.slice(0, 50));
    const pythonAnomalies = detectAnomalies(pythonTimes.slice(0, 50));

    // Create point styles for anomaly highlighting
    const dotnetPointStyles = dotnetTimes.slice(0, 50).map((_, idx) => 
        dotnetAnomalies.includes(idx) ? 'triangle' : 'circle'
    );
    const dotnetPointRadius = dotnetTimes.slice(0, 50).map((_, idx) => 
        dotnetAnomalies.includes(idx) ? 6 : 3
    );
    const dotnetPointColors = dotnetTimes.slice(0, 50).map((_, idx) => 
        dotnetAnomalies.includes(idx) ? '#dc3545' : '#0d6efd'
    );

    const pythonPointStyles = pythonTimes.slice(0, 50).map((_, idx) => 
        pythonAnomalies.includes(idx) ? 'triangle' : 'circle'
    );
    const pythonPointRadius = pythonTimes.slice(0, 50).map((_, idx) => 
        pythonAnomalies.includes(idx) ? 6 : 3
    );
    const pythonPointColors = pythonTimes.slice(0, 50).map((_, idx) => 
        pythonAnomalies.includes(idx) ? '#dc3545' : '#fd7e14'
    );

    charts.trends = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels.slice(0, 50), // Limit to 50 points for readability
            datasets: [
                {
                    label: '.NET (ms)',
                    data: dotnetTimes.slice(0, 50),
                    borderColor: '#0d6efd',
                    backgroundColor: 'rgba(13, 110, 253, 0.1)',
                    tension: 0.4,
                    borderWidth: 2,
                    pointStyle: dotnetPointStyles,
                    pointRadius: dotnetPointRadius,
                    pointBackgroundColor: dotnetPointColors,
                    pointBorderColor: dotnetPointColors,
                    fill: true
                },
                {
                    label: 'Python (ms)',
                    data: pythonTimes.slice(0, 50),
                    borderColor: '#fd7e14',
                    backgroundColor: 'rgba(253, 126, 20, 0.1)',
                    tension: 0.4,
                    borderWidth: 2,
                    pointStyle: pythonPointStyles,
                    pointRadius: pythonPointRadius,
                    pointBackgroundColor: pythonPointColors,
                    pointBorderColor: pythonPointColors,
                    fill: true
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'top'
                },
                title: {
                    display: true,
                    text: 'Iteration Time Trends (🔺 = Anomalies)'
                },
                tooltip: {
                    callbacks: {
                        afterLabel: function(context) {
                            const idx = context.dataIndex;
                            const datasetIdx = context.datasetIndex;
                            if (datasetIdx === 0 && dotnetAnomalies.includes(idx)) {
                                return '⚠️ Anomaly detected';
                            }
                            if (datasetIdx === 1 && pythonAnomalies.includes(idx)) {
                                return '⚠️ Anomaly detected';
                            }
                            return '';
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Time (ms)'
                    }
                },
                x: {
                    title: {
                        display: true,
                        text: 'Iteration Number'
                    }
                }
            }
        }
    });
};

// Render distribution histogram
window.renderDistributionChart = async function (labels, dotnetCounts, pythonCounts) {
    await loadChartJs();

    const ctx = document.getElementById('distributionChart');
    if (!ctx) return;

    if (charts.distribution) {
        charts.distribution.destroy();
    }

    charts.distribution = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: '.NET',
                    data: dotnetCounts,
                    backgroundColor: '#0d6efd',
                    borderColor: '#0d6efd',
                    borderWidth: 1
                },
                {
                    label: 'Python',
                    data: pythonCounts,
                    backgroundColor: '#fd7e14',
                    borderColor: '#fd7e14',
                    borderWidth: 1
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'top'
                },
                title: {
                    display: true,
                    text: 'Iteration Time Distribution'
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Frequency'
                    }
                },
                x: {
                    title: {
                        display: true,
                        text: 'Time Range (ms)'
                    }
                }
            }
        }
    });
};

// Render memory usage chart
window.renderMemoryChart = async function (labels, dotnetMemory, pythonMemory) {
    await loadChartJs();

    const ctx = document.getElementById('memoryChart');
    if (!ctx) return;

    if (charts.memory) {
        charts.memory.destroy();
    }

    charts.memory = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: '.NET Memory (MB)',
                    data: dotnetMemory,
                    borderColor: '#0d6efd',
                    backgroundColor: 'rgba(13, 110, 253, 0.1)',
                    fill: true,
                    tension: 0.4,
                    borderWidth: 2,
                    pointRadius: 0
                },
                {
                    label: 'Python Memory (MB)',
                    data: pythonMemory,
                    borderColor: '#fd7e14',
                    backgroundColor: 'rgba(253, 126, 20, 0.1)',
                    fill: true,
                    tension: 0.4,
                    borderWidth: 2,
                    pointRadius: 0
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'top'
                },
                title: {
                    display: true,
                    text: 'Memory Usage Over Time'
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Memory (MB)'
                    }
                },
                x: {
                    title: {
                        display: true,
                        text: 'Iterations'
                    }
                }
            }
        }
    });
};

// Render comparison charts (side-by-side)
window.renderComparisonCharts = async function (avgLabels, avgValues, memLabels, memValues) {
    await loadChartJs();

    // Average time comparison
    const avgCtx = document.getElementById('avgComparisonChart');
    if (avgCtx) {
        if (charts.avgComparison) {
            charts.avgComparison.destroy();
        }

        charts.avgComparison = new Chart(avgCtx, {
            type: 'bar',
            data: {
                labels: avgLabels,
                datasets: [
                    {
                        label: 'Avg Time (ms)',
                        data: avgValues,
                        backgroundColor: ['#0d6efd', '#fd7e14'],
                        borderColor: ['#0d6efd', '#fd7e14'],
                        borderWidth: 2
                    }
                ]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    },
                    title: {
                        display: true,
                        text: 'Average Time per Iteration'
                    }
                },
                scales: {
                    x: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Time (ms)'
                        }
                    }
                }
            }
        });
    }

    // Memory comparison
    const memCtx = document.getElementById('memComparisonChart');
    if (memCtx) {
        if (charts.memComparison) {
            charts.memComparison.destroy();
        }

        charts.memComparison = new Chart(memCtx, {
            type: 'bar',
            data: {
                labels: memLabels,
                datasets: [
                    {
                        label: 'Memory (MB)',
                        data: memValues,
                        backgroundColor: ['#0d6efd', '#fd7e14'],
                        borderColor: ['#0d6efd', '#fd7e14'],
                        borderWidth: 2
                    }
                ]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    },
                    title: {
                        display: true,
                        text: 'Memory Usage'
                    }
                },
                scales: {
                    x: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Memory (MB)'
                        }
                    }
                }
            }
        });
    }
};

// Render gauge charts for comparison
window.renderGaugeCharts = async function (dotnetAvg, pythonAvg, dotnetMemory, pythonMemory, dotnetConsistency, pythonConsistency) {
    await loadChartJs();

    // Performance Gauge (Average Time)
    const perfCtx = document.getElementById('performanceGauge');
    if (perfCtx) {
        if (charts.performanceGauge) {
            charts.performanceGauge.destroy();
        }

        // Calculate scores (lower is better for time, scale to 0-100)
        const maxTime = Math.max(dotnetAvg, pythonAvg);
        const dotnetScore = Math.round((1 - dotnetAvg / maxTime) * 100);
        const pythonScore = Math.round((1 - pythonAvg / maxTime) * 100);

        charts.performanceGauge = new Chart(perfCtx, {
            type: 'doughnut',
            data: {
                labels: ['.NET', 'Python'],
                datasets: [{
                    data: [dotnetScore, pythonScore],
                    backgroundColor: ['#0d6efd', '#fd7e14'],
                    borderWidth: 2,
                    circumference: 180,
                    rotation: 270
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true,
                        position: 'bottom'
                    },
                    title: {
                        display: true,
                        text: 'Performance Index'
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return context.label + ': ' + context.parsed + '/100';
                            }
                        }
                    }
                }
            }
        });
    }

    // Memory Efficiency Gauge
    const memCtx = document.getElementById('memoryGauge');
    if (memCtx) {
        if (charts.memoryGauge) {
            charts.memoryGauge.destroy();
        }

        // Calculate scores (lower memory is better, scale to 0-100)
        const maxMemory = Math.max(dotnetMemory, pythonMemory);
        const dotnetMemScore = Math.round((1 - dotnetMemory / maxMemory) * 100);
        const pythonMemScore = Math.round((1 - pythonMemory / maxMemory) * 100);

        charts.memoryGauge = new Chart(memCtx, {
            type: 'doughnut',
            data: {
                labels: ['.NET', 'Python'],
                datasets: [{
                    data: [dotnetMemScore, pythonMemScore],
                    backgroundColor: ['#0d6efd', '#fd7e14'],
                    borderWidth: 2,
                    circumference: 180,
                    rotation: 270
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true,
                        position: 'bottom'
                    },
                    title: {
                        display: true,
                        text: 'Memory Efficiency'
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return context.label + ': ' + context.parsed + '/100';
                            }
                        }
                    }
                }
            }
        });
    }

    // Consistency Gauge
    const consCtx = document.getElementById('consistencyGauge');
    if (consCtx) {
        if (charts.consistencyGauge) {
            charts.consistencyGauge.destroy();
        }

        charts.consistencyGauge = new Chart(consCtx, {
            type: 'doughnut',
            data: {
                labels: ['.NET', 'Python'],
                datasets: [{
                    data: [dotnetConsistency, pythonConsistency],
                    backgroundColor: ['#0d6efd', '#fd7e14'],
                    borderWidth: 2,
                    circumference: 180,
                    rotation: 270
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true,
                        position: 'bottom'
                    },
                    title: {
                        display: true,
                        text: 'Consistency Score'
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return context.label + ': ' + context.parsed + '%';
                            }
                        }
                    }
                }
            }
        });
    }
};

// Download file utility
window.downloadFile = function (filename, content) {
    const element = document.createElement('a');
    element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(content));
    element.setAttribute('download', filename);
    element.style.display = 'none';
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
};

// Dark mode toggle utilities
window.themeManager = {
    // Initialize theme from localStorage or system preference
    init: function() {
        const savedTheme = localStorage.getItem('theme');
        const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        const theme = savedTheme || (prefersDark ? 'dark' : 'light');
        this.setTheme(theme);
    },
    
    // Set theme and persist to localStorage
    setTheme: function(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        localStorage.setItem('theme', theme);
        
        // Update chart colors based on theme
        if (typeof Chart !== 'undefined') {
            Chart.defaults.color = theme === 'dark' ? '#e0e0e0' : '#666';
            Chart.defaults.borderColor = theme === 'dark' ? '#404040' : '#ddd';
        }
    },
    
    // Toggle between light and dark
    toggle: function() {
        const currentTheme = document.documentElement.getAttribute('data-bs-theme');
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        this.setTheme(newTheme);
        return newTheme;
    },
    
    // Get current theme
    getTheme: function() {
        return document.documentElement.getAttribute('data-bs-theme') || 'light';
    }
};

// Initialize theme on load
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => window.themeManager.init());
} else {
    window.themeManager.init();
}
