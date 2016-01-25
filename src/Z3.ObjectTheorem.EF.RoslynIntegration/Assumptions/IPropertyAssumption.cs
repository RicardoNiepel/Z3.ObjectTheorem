using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Z3.ObjectTheorem.Solving;

namespace Z3.ObjectTheorem.EF.RoslynIntegration.Assumptions
{
    public interface IPropertyAssumption
    {
        Expression<Func<bool>> Assumption { get; }

        IPropertySymbol Property { get; }

        IEnumerable<string> GetAttributesToDelete(ObjectTheoremResult objectTheoremResult);

        IEnumerable<string> GetAttributesToAdd(ObjectTheoremResult objectTheoremResult);

        IEnumerable<AttributeSyntax> GetAttributeSyntaxexToAdd(ObjectTheoremResult objectTheoremResult);

        int Rank { get; }
    }
}