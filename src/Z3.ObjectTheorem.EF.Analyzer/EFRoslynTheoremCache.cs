using Microsoft.CodeAnalysis;
using System.Runtime.Caching;
using Z3.ObjectTheorem.EF.RoslynIntegration;
using Z3.ObjectTheorem.Solving;

namespace Z3.ObjectTheorem.EF.Analyzer
{
    public static class EFRoslynTheoremCache
    {
        private static readonly ObjectCache _cache = MemoryCache.Default;

        public static string Add(Compilation compilation, EFRoslynTheorem efRoslynTheorem, ObjectTheoremResult objectTheoremResult)
        {
            var cacheId = compilation.AssemblyName;
            _cache[cacheId] = new Item(compilation, efRoslynTheorem, objectTheoremResult);
            return cacheId;
        }

        public static Item Get(string cacheId)
        {
            return (Item)_cache[cacheId];
        }

        public class Item
        {
            public Item(Compilation compilation, EFRoslynTheorem efRoslynTheorem, ObjectTheoremResult objectTheoremResult)
            {
                Compilation = compilation;
                EFRoslynTheorem = efRoslynTheorem;
                ObjectTheoremResult = objectTheoremResult;
            }

            public Compilation Compilation { get; }

            public EFRoslynTheorem EFRoslynTheorem { get; }

            public ObjectTheoremResult ObjectTheoremResult { get; }
        }
    }
}