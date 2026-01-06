// PDF Export functionality using jsPDF and html2canvas
// These libraries will be loaded dynamically when needed

let pdfLibsLoaded = false;

// Load jsPDF and html2canvas libraries
async function loadPdfLibraries() {
    if (pdfLibsLoaded) return;

    return new Promise((resolve, reject) => {
        // Load html2canvas first
        const html2canvasScript = document.createElement('script');
        html2canvasScript.src = 'https://cdn.jsdelivr.net/npm/html2canvas@1.4.1/dist/html2canvas.min.js';
        html2canvasScript.async = true;
        
        html2canvasScript.onload = () => {
            // Then load jsPDF
            const jsPdfScript = document.createElement('script');
            jsPdfScript.src = 'https://cdn.jsdelivr.net/npm/jspdf@2.5.1/dist/jspdf.umd.min.js';
            jsPdfScript.async = true;
            
            jsPdfScript.onload = () => {
                pdfLibsLoaded = true;
                resolve();
            };
            
            jsPdfScript.onerror = () => reject(new Error('Failed to load jsPDF'));
            document.head.appendChild(jsPdfScript);
        };
        
        html2canvasScript.onerror = () => reject(new Error('Failed to load html2canvas'));
        document.head.appendChild(html2canvasScript);
    });
}

// Export performance dashboard to PDF
window.exportToPdf = async function(dotnetData, pythonData, insights) {
    try {
        await loadPdfLibraries();
        
        const { jsPDF } = window.jspdf;
        const doc = new jsPDF('p', 'mm', 'a4');
        
        const pageWidth = doc.internal.pageSize.getWidth();
        const pageHeight = doc.internal.pageSize.getHeight();
        const margin = 15;
        let yPos = margin;
        
        // Title
        doc.setFontSize(20);
        doc.setFont(undefined, 'bold');
        doc.text('Performance Comparison Report', pageWidth / 2, yPos, { align: 'center' });
        yPos += 10;
        
        // Timestamp
        doc.setFontSize(10);
        doc.setFont(undefined, 'normal');
        doc.text(`Generated: ${new Date().toLocaleString()}`, pageWidth / 2, yPos, { align: 'center' });
        yPos += 15;
        
        // Winner Banner
        doc.setFontSize(14);
        doc.setFont(undefined, 'bold');
        doc.setFillColor(0, 109, 253);
        doc.rect(margin, yPos - 5, pageWidth - 2 * margin, 12, 'F');
        doc.setTextColor(255, 255, 255);
        doc.text(insights.winnerFramework + ' is ' + insights.percentageFaster.toFixed(1) + '% Faster', 
                 pageWidth / 2, yPos + 3, { align: 'center' });
        doc.setTextColor(0, 0, 0);
        yPos += 20;
        
        // .NET Results
        doc.setFontSize(16);
        doc.setFont(undefined, 'bold');
        doc.setTextColor(13, 110, 253); // Blue
        doc.text('.NET Results', margin, yPos);
        yPos += 8;
        
        doc.setFontSize(10);
        doc.setFont(undefined, 'normal');
        doc.setTextColor(0, 0, 0);
        doc.text(`Average Time: ${dotnetData.averageTimePerIterationMs.toFixed(2)} ms`, margin + 5, yPos);
        yPos += 6;
        doc.text(`Min Time: ${dotnetData.minIterationTimeMs.toFixed(2)} ms`, margin + 5, yPos);
        yPos += 6;
        doc.text(`Max Time: ${dotnetData.maxIterationTimeMs.toFixed(2)} ms`, margin + 5, yPos);
        yPos += 6;
        doc.text(`Total Time: ${dotnetData.totalExecutionTimeMs.toFixed(2)} ms`, margin + 5, yPos);
        yPos += 6;
        doc.text(`Memory Used: ${dotnetData.memoryUsedMB.toFixed(2)} MB`, margin + 5, yPos);
        yPos += 6;
        doc.text(`Iterations: ${dotnetData.currentIteration}`, margin + 5, yPos);
        yPos += 12;
        
        // Python Results
        doc.setFontSize(16);
        doc.setFont(undefined, 'bold');
        doc.setTextColor(253, 126, 20); // Orange
        doc.text('Python Results', margin, yPos);
        yPos += 8;
        
        doc.setFontSize(10);
        doc.setFont(undefined, 'normal');
        doc.setTextColor(0, 0, 0);
        doc.text(`Average Time: ${pythonData.averageTimePerIterationMs.toFixed(2)} ms`, margin + 5, yPos);
        yPos += 6;
        doc.text(`Min Time: ${pythonData.minIterationTimeMs.toFixed(2)} ms`, margin + 5, yPos);
        yPos += 6;
        doc.text(`Max Time: ${pythonData.maxIterationTimeMs.toFixed(2)} ms`, margin + 5, yPos);
        yPos += 6;
        doc.text(`Total Time: ${pythonData.totalExecutionTimeMs.toFixed(2)} ms`, margin + 5, yPos);
        yPos += 6;
        doc.text(`Memory Used: ${pythonData.memoryUsedMB.toFixed(2)} MB`, margin + 5, yPos);
        yPos += 6;
        doc.text(`Iterations: ${pythonData.currentIteration}`, margin + 5, yPos);
        yPos += 12;
        
        // Insights
        if (yPos > pageHeight - 60) {
            doc.addPage();
            yPos = margin;
        }
        
        doc.setFontSize(16);
        doc.setFont(undefined, 'bold');
        doc.setTextColor(40, 167, 69); // Green
        doc.text('AI Insights', margin, yPos);
        yPos += 8;
        
        doc.setFontSize(10);
        doc.setFont(undefined, 'normal');
        doc.setTextColor(0, 0, 0);
        
        // Main insight
        const splitMainInsight = doc.splitTextToSize(insights.mainInsight, pageWidth - 2 * margin - 10);
        doc.text(splitMainInsight, margin + 5, yPos);
        yPos += splitMainInsight.length * 6 + 6;
        
        // Key findings
        if (insights.keyFindings && insights.keyFindings.length > 0) {
            doc.setFont(undefined, 'bold');
            doc.text('Key Findings:', margin + 5, yPos);
            yPos += 6;
            doc.setFont(undefined, 'normal');
            
            insights.keyFindings.forEach(finding => {
                if (yPos > pageHeight - 20) {
                    doc.addPage();
                    yPos = margin;
                }
                const splitFinding = doc.splitTextToSize('• ' + finding, pageWidth - 2 * margin - 10);
                doc.text(splitFinding, margin + 10, yPos);
                yPos += splitFinding.length * 6 + 2;
            });
        }
        
        // Save PDF
        const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, -5);
        doc.save(`performance-comparison-${timestamp}.pdf`);
        
        return true;
    } catch (error) {
        console.error('PDF export error:', error);
        return false;
    }
};
