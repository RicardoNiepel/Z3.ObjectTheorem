using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Z3.ObjectTheorem.EF.RoslynIntegration;
using Z3.ObjectTheorem.EF.RoslynIntegration.Assumptions;
using Z3.ObjectTheorem.Solving;

namespace Z3.ObjectTheorem.EF.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class Z3ObjectTheoremEFAnalyzer : DiagnosticAnalyzer
    {
        public const string SatisfiableDiagnosticId = "Z3ObjectTheoremEFAnalyzer_Satisfiable";
        public const string UnsatisfiableDiagnosticId = "Z3ObjectTheoremEFAnalyzer_Unsatisfiable";

        private const string Category = "Z3ObjectTheorem";

        private static DiagnosticDescriptor SatisfiableRule =
            new DiagnosticDescriptor(SatisfiableDiagnosticId, "EF model is invalid", "EF model {0} is invalid but can be fixed automatically", Category,
                DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "Some of the EF constraint assumptions are wrong.");

        private static DiagnosticDescriptor UnsatisfiableRule =
            new DiagnosticDescriptor(UnsatisfiableDiagnosticId, "EF model is invalid", "EF model {0} is invalid and cannot be fixed automatically", Category,
                DiagnosticSeverity.Error, isEnabledByDefault: true, description: "Some of the EF constraint assumptions are wrong.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(SatisfiableRule, UnsatisfiableRule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(AnalyzeCompilation);
        }

        private void AnalyzeCompilation(CompilationAnalysisContext context)
        {
            Trace.WriteLine("AnalyzeCompilation");

            try
            {
                INamedTypeSymbol dbContextTypeSymbol;
                IEnumerable<INamedTypeSymbol> entityTypeSymbols = SymbolHelper.GetAllEntityTypesFromDbContext(context.Compilation, out dbContextTypeSymbol);
                if (!entityTypeSymbols.Any())
                {
                    return;
                }

                var allTypeSymbols = context.Compilation.GetSymbolsWithName(s => !s.EndsWith("DbContext"), SymbolFilter.Type).Cast<INamedTypeSymbol>();
                var allMemberSymbols = allTypeSymbols.SelectMany(t => t.GetMembers().Where(m => m.Kind == SymbolKind.Property));
                Trace.WriteLine("Class count: " + allTypeSymbols.Count());
                Trace.WriteLine("Property count: " + allMemberSymbols.Count());

                var efRoslynTheorem = new EFRoslynTheorem();
                var result = efRoslynTheorem.Solve(entityTypeSymbols);
                if (result.Status == Status.Unsatisfiable)
                {
                    var classAssumptions = efRoslynTheorem.ClassAssumptions.ToList();
                    var propertyAssumptions = efRoslynTheorem.PropertyAssumptions.OrderBy(pa => pa.Rank).ToList();

                    do
                    {
                        result = TryToRemoveWrongAssumption(efRoslynTheorem, result, classAssumptions, propertyAssumptions);
                    } while (result != null && result.Status != Status.Satisfiable);

                    if (result == null || result.Status != Status.Satisfiable)
                    {
                        var diagnostic2 = Diagnostic.Create(UnsatisfiableRule, dbContextTypeSymbol.Locations[0], dbContextTypeSymbol.Name);
                        Trace.WriteLine("ReportDiagnostic " + diagnostic2.Descriptor.Id);
                        context.ReportDiagnostic(diagnostic2);
                        return;
                    }

                    var cacheId = EFRoslynTheoremCache.Add(context.Compilation, efRoslynTheorem, result);

                    var props = ImmutableDictionary.Create<string, string>();
                    props = props.Add("CacheId", cacheId);

                    var diagnostic = Diagnostic.Create(SatisfiableRule, dbContextTypeSymbol.Locations[0], props, dbContextTypeSymbol.Name);
                    Trace.WriteLine("ReportDiagnostic " + diagnostic.Descriptor.Id);
                    context.ReportDiagnostic(diagnostic);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                throw;
            }
        }

        private ObjectTheoremResult TryToRemoveWrongAssumption(EFRoslynTheorem efRoslynTheorem, ObjectTheoremResult result,
            List<IClassAssumption> classAssumptions, List<IPropertyAssumption> propertyAssumptions)
        {
            foreach (var assumes in classAssumptions)
            {
                if (result.IsAssumptionWrong(assumes.Assumption))
                {
                    classAssumptions.Remove(assumes);
                    efRoslynTheorem.RemoveAssumption(assumes.Assumption);
                    return efRoslynTheorem.ReSolve();
                }
            }

            foreach (var assumes in propertyAssumptions)
            {
                if (result.IsAssumptionWrong(assumes.Assumption))
                {
                    propertyAssumptions.Remove(assumes);
                    efRoslynTheorem.RemoveAssumption(assumes.Assumption);
                    return efRoslynTheorem.ReSolve();
                }
            }

            return null;
        }
    }
}