using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Z3.ObjectTheorem.EF.RoslynIntegration
{
    public static class SymbolHelper
    {
        public static IEnumerable<INamedTypeSymbol> GetAllEntityTypesFromDbContext(Compilation compilation, out INamedTypeSymbol dbContextTypeSymbol)
        {
            var allTypeSymbols = compilation.GetSymbolsWithName(s => true, SymbolFilter.Type).Cast<INamedTypeSymbol>();

            dbContextTypeSymbol = allTypeSymbols.Single(t => t.BaseType.Name == "DbContext");
            var entityTypeSymbols = dbContextTypeSymbol
                .GetMembers()
                .Where(m => m.Kind == SymbolKind.Property)
                .Cast<IPropertySymbol>()
                .Where(p => p.Type.Name == "DbSet")
                .Select(p => p.Type)
                .Cast<INamedTypeSymbol>()
                .Select(t => t.TypeArguments[0])
                .Cast<INamedTypeSymbol>();

            return entityTypeSymbols;
        }
    }
}