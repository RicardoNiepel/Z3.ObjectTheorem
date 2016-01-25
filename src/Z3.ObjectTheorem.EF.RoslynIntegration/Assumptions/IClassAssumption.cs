using Microsoft.CodeAnalysis;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using Z3.ObjectTheorem.Solving;

namespace Z3.ObjectTheorem.EF.RoslynIntegration.Assumptions
{
    public interface IClassAssumption
    {
        Expression<Func<bool>> Assumption { get; }

        INamedTypeSymbol Class { get; }

        int Rank { get; }

        IEnumerable<string> GetAttributesToDelete(ObjectTheoremResult objectTheoremResult);

        IEnumerable<string> GetAttributesToAdd(ObjectTheoremResult objectTheoremResult);
    }
}