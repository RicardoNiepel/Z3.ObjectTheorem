using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using TestHelper;

namespace Z3.ObjectTheorem.EF.Analyzer.Tests
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        [TestMethod]
        [TestCategory("EFScenario.AdventureWorksLT")]
        public async Task Z3ObjectTheoremEFAnalyzer_AdventureWorksLT150_ShouldBeDiagnosed()
        {
            var badProjectFilePath = @"..\..\..\EFScenario.AdventureWorksLT150\EFScenario.AdventureWorksLT150.csproj";

            var expected = new DiagnosticResult
            {
                Id = "Z3ObjectTheoremEFAnalyzer_Satisfiable",
                Message = String.Format("EF model {0} is invalid but can be fixed automatically", "AdventureWorksLTDbContext"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation(Path.GetFullPath(@"..\..\..\EFScenario.AdventureWorksLT150\AdventureWorksLTDbContext.cs"), 8, 26)
                        }
            };

            await VerifyDiagnosticFromProjectAsync(badProjectFilePath, expected);
        }

        [TestMethod]
        [TestCategory("EFScenario.AdventureWorksLT")]
        public async Task Z3ObjectTheoremEFAnalyzer_AdventureWorksLT200_ShouldBeDiagnosed()
        {
            var badProjectFilePath = @"..\..\..\EFScenario.AdventureWorksLT200\EFScenario.AdventureWorksLT200.csproj";

            var expected = new DiagnosticResult
            {
                Id = "Z3ObjectTheoremEFAnalyzer_Satisfiable",
                Message = String.Format("EF model {0} is invalid but can be fixed automatically", "AdventureWorksLTDbContext"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation(Path.GetFullPath(@"..\..\..\EFScenario.AdventureWorksLT200\AdventureWorksLTDbContext.cs"), 8, 26)
                        }
            };

            await VerifyDiagnosticFromProjectAsync(badProjectFilePath, expected);
        }

        [TestMethod]
        [TestCategory("EFScenario.AdventureWorksLT")]
        public async Task Z3ObjectTheoremEFAnalyzer_AdventureWorksLTProject_ShouldBeDiagnosed()
        {
            var badProjectFilePath = @"..\..\..\EFScenario.AdventureWorksLT\EFScenario.AdventureWorksLT.csproj";

            var expected = new DiagnosticResult
            {
                Id = "Z3ObjectTheoremEFAnalyzer_Satisfiable",
                Message = String.Format("EF model {0} is invalid but can be fixed automatically", "AdventureWorksLTDbContext"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation(Path.GetFullPath(@"..\..\..\EFScenario.AdventureWorksLT\AdventureWorksLTDbContext.cs"), 8, 26)
                        }
            };

            await VerifyDiagnosticFromProjectAsync(badProjectFilePath, expected);
        }

        [TestMethod]
        public async Task Z3ObjectTheoremEFAnalyzer_BadProject_ShouldBeFixedTo_GoodProject()
        {
            var badProjectFilePath = @"..\..\..\EFScenario.Bad\EFScenario.Bad.csproj";
            var goodProjectFilePath = @"..\..\..\EFScenario.Good\EFScenario.Good.csproj";

            var expected = new DiagnosticResult
            {
                Id = "Z3ObjectTheoremEFAnalyzer_Satisfiable",
                Message = String.Format("EF model {0} is invalid but can be fixed automatically", "EFScenarioDbContext"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation(Path.GetFullPath(@"..\..\..\EFScenario.Bad\EFScenarioDbContext.cs"), 6, 18)
                        }
            };

            await VerifyDiagnosticFromProjectAsync(badProjectFilePath, expected);

            await VerifyCSharpFixFromProjectsAsync(badProjectFilePath, goodProjectFilePath);
        }

        //No diagnostics expected to show up
        [TestMethod]
        public void Z3ObjectTheoremEFAnalyzer_NoSourceCode_ShouldShow_NoDiagnostics_()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new Z3ObjectTheoremEFAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new Z3ObjectTheoremEFAnalyzer();
        }
    }
}