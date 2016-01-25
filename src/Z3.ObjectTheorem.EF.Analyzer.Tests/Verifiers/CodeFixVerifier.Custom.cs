using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestHelper
{
    public abstract partial class CodeFixVerifier : DiagnosticVerifier
    {
        protected Task VerifyCSharpFixFromProjectsAsync(string oldProjectFilePath, string newProjectFilePath, int? codeFixIndex = null)
        {
            return VerifyCSharpFixFromProjectsAsync(GetCSharpDiagnosticAnalyzer(), GetCSharpCodeFixProvider(), oldProjectFilePath, newProjectFilePath, codeFixIndex);
        }

        private async Task VerifyCSharpFixFromProjectsAsync(DiagnosticAnalyzer analyzer, CodeFixProvider codeFixProvider, string oldProjectFilePath, string newProjectFilePath, int? codeFixIndex)
        {
            var oldDocuments = await GetDocumentsAsync(oldProjectFilePath);
            var newDocuments = await GetDocumentsAsync(newProjectFilePath);

            Solution newSolution = null;

            var analyzerDiagnostics = GetSortedDiagnosticsFromDocuments(analyzer, oldDocuments);
            var attempts = analyzerDiagnostics.Length;

            for (int i = 0; i < attempts; ++i)
            {
                var actions = new List<CodeAction>();
                var context = new CodeFixContext(oldDocuments[0], analyzerDiagnostics[0], (a, d) => actions.Add(a), CancellationToken.None);
                codeFixProvider.RegisterCodeFixesAsync(context).Wait();

                if (!actions.Any())
                {
                    break;
                }

                if (codeFixIndex != null)
                {
                    newSolution = ApplyFix(actions.ElementAt((int)codeFixIndex));
                    break;
                }

                newSolution = ApplyFix(actions.ElementAt(0));
                analyzerDiagnostics = GetSortedDiagnosticsFromDocuments(analyzer, oldDocuments);

                //check if there are analyzer diagnostics left after the code fix
                if (!analyzerDiagnostics.Any())
                {
                    break;
                }
            }

            var errors = new List<Exception>();

            foreach (var oldDocument in oldDocuments.Where(d => d.Name != "AssemblyInfo.cs"))
            {
                var fixedDocument = newSolution.Projects.Single(p => p.Name == oldDocument.Project.Name).Documents.Single(d => d.Name == oldDocument.Name);
                var expectedDocument = newDocuments.Single(d => d.Name == oldDocument.Name);

                //after applying all of the code fixes, compare the resulting string to the inputted one
                var actual = GetStringFromDocument(fixedDocument);
                var expected = GetStringFromDocument(expectedDocument);

                try
                {
                    Assert.AreEqual(expected, actual);
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }

            if (errors.Count > 0)
            {
                Assert.Fail(string.Join("\n\n", errors.Select(e => e.Message)));
            }
        }

        private static Solution ApplyFix(CodeAction codeAction)
        {
            var operations = codeAction.GetOperationsAsync(CancellationToken.None).Result;
            var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
            return solution;
        }
    }
}