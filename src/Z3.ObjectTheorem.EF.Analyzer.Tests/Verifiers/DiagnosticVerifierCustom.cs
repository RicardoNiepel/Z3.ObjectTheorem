using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.Linq;
using System.Threading.Tasks;

namespace TestHelper
{
    /// <summary>
    /// Custom class to extend class DiagnosticVerifier from default template
    /// </summary>
    public abstract partial class DiagnosticVerifier
    {
        protected async Task VerifyDiagnosticFromProjectAsync(string projectFilePath, params DiagnosticResult[] expected)
        {
            var analyzer = GetCSharpDiagnosticAnalyzer();

            var documents = await GetDocumentsAsync(projectFilePath);
            var diagnostics = GetSortedDiagnosticsFromDocuments(analyzer, documents);

            VerifyDiagnosticResults(diagnostics, analyzer, expected);
        }

        protected async Task<Document[]> GetDocumentsAsync(string projectFilePath)
        {
            var msWorkspace = MSBuildWorkspace.Create();
            var project = await msWorkspace.OpenProjectAsync(projectFilePath);
            return project.Documents.ToArray();
        }
    }
}