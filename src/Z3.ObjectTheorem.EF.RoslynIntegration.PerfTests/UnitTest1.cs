using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Z3.ObjectTheorem.Solving;
using System.Linq;
using System.Diagnostics;

namespace Z3.ObjectTheorem.EF.RoslynIntegration.PerfTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            // Arrange
            var projectFilePath = @"..\..\..\Z3.ObjectTheorem.EF.RoslynIntegration.PerfTests\Z3.ObjectTheorem.EF.RoslynIntegration.PerfTests.csproj";

            var compilation = await LoadProjectAndGetCompilationAsync(projectFilePath);

            var scenarioClasses = compilation.GetSymbolsWithName(s => s.StartsWith("ScenarioClass"), SymbolFilter.Type).Cast<INamedTypeSymbol>();
            var allMemberSymbols = scenarioClasses.SelectMany(t => t.GetMembers().Where(m => m.Kind == SymbolKind.Property));
            Trace.WriteLine("Class count: " + scenarioClasses.Count());
            Trace.WriteLine("Total Property count: " + allMemberSymbols.Count());
            Trace.WriteLine("Property per class count: " + allMemberSymbols.Count() / scenarioClasses.Count());
            Trace.WriteLine("");

            // Act
            var efRoslynTheorem = new EFRoslynTheorem();
            ObjectTheoremResult solved = efRoslynTheorem.Solve(scenarioClasses);

            // Assert
            Assert.IsNotNull(solved);
            Assert.AreEqual(Status.Unsatisfiable, solved.Status);

            foreach (var assumes in efRoslynTheorem.ClassAssumptions)
            {
                efRoslynTheorem.RemoveAssumption(assumes.Assumption);
            }

            foreach (var assumes in efRoslynTheorem.PropertyAssumptions)
            {
                efRoslynTheorem.RemoveAssumption(assumes.Assumption);
            }

            Trace.WriteLine("");
            solved = efRoslynTheorem.ReSolve();
            Assert.IsNotNull(solved);
            Assert.AreEqual(Status.Satisfiable, solved.Status);
        }

        private async Task<Compilation> LoadProjectAndGetCompilationAsync(string projectFilePath)
        {
            var msWorkspace = MSBuildWorkspace.Create();
            var project = await msWorkspace.OpenProjectAsync(projectFilePath);
            return await project.GetCompilationAsync();
        }
    }
}
