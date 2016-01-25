using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Z3.ObjectTheorem.EF.RoslynIntegration.Assumptions;
using Z3.ObjectTheorem.Solving;

namespace Z3.ObjectTheorem.EF.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Z3ObjectTheoremEFAnalyzerCodeFixProvider)), Shared]
    public class Z3ObjectTheoremEFAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Fix EF model";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(Z3ObjectTheoremEFAnalyzer.SatisfiableDiagnosticId); }
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Trace.WriteLine("RegisterCodeFixesAsync");
            try
            {
                var diagnostic = context.Diagnostics.First();

                var cacheItem = EFRoslynTheoremCache.Get(diagnostic.Properties["CacheId"]);

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedSolution: cancellationToken =>
                            FixEFModelAsync(context.Document.Project, cacheItem.Compilation,
                                cacheItem.EFRoslynTheorem.ClassAssumptions, cacheItem.EFRoslynTheorem.PropertyAssumptions, cacheItem.ObjectTheoremResult,
                                cancellationToken),
                        equivalenceKey: title),
                    diagnostic);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                throw;
            }
        }

        private async Task<Solution> FixEFModelAsync(Project project, Compilation compilation,
            IEnumerable<IClassAssumption> classAssumptions, IEnumerable<IPropertyAssumption> propertyAssumptions, ObjectTheoremResult objectTheoremResult,
            CancellationToken cancellationToken)
        {
            Trace.WriteLine("FixEFModelAsync");
            try
            {
                var newSolution = project.Solution;

                foreach (var syntaxTree in compilation.SyntaxTrees)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var syntaxRoot = await syntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
                    var semanticModel = compilation.GetSemanticModel(syntaxTree);

                    var rewriter = new AttributeAssumptionsRewriter(semanticModel, classAssumptions, propertyAssumptions, objectTheoremResult, cancellationToken);
                    var newSyntaxRoot = rewriter.Visit(syntaxRoot);

                    cancellationToken.ThrowIfCancellationRequested();

                    var documentId = newSolution.GetDocumentId(syntaxTree);
                    newSolution = newSolution.WithDocumentSyntaxRoot(documentId, newSyntaxRoot);
                }

                return newSolution;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}