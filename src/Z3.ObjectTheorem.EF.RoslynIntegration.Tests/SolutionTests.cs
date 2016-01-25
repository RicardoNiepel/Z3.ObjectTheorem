using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Z3.ObjectTheorem.Solving;
using System.Linq;

namespace Z3.ObjectTheorem.EF.RoslynIntegration.Tests
{
    [TestClass]
    public class SolutionTests
    {
        [TestMethod]
        public async Task RoslynWith_EFScenarioBad()
        {
            // Arrange
            var projectFilePath = @"..\..\..\EFScenario.Bad\EFScenario.Bad.csproj";

            var compilation = await LoadProjectAndGetCompilationAsync(projectFilePath);
            INamedTypeSymbol dbContextTypeSymbol;
            var entityTypeSymbols = SymbolHelper.GetAllEntityTypesFromDbContext(compilation, out dbContextTypeSymbol);

            // Act
            var efRoslynTheorem = new EFRoslynTheorem();
            ObjectTheoremResult solved = efRoslynTheorem.Solve(entityTypeSymbols);

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

            solved = efRoslynTheorem.ReSolve();
            Assert.IsNotNull(solved);
            Assert.AreEqual(Status.Satisfiable, solved.Status);
        }

        [TestMethod]
        public async Task RoslynWith_EFScenarioGood()
        {
            // Arrange
            var projectFilePath = @"..\..\..\EFScenario.Good\EFScenario.Good.csproj";

            var compilation = await LoadProjectAndGetCompilationAsync(projectFilePath);
            INamedTypeSymbol dbContextTypeSymbol;
            var entityTypeSymbols = SymbolHelper.GetAllEntityTypesFromDbContext(compilation, out dbContextTypeSymbol);

            // Act
            var efRoslynTheorem = new EFRoslynTheorem();
            ObjectTheoremResult solved = efRoslynTheorem.Solve(entityTypeSymbols);

            // Assert
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