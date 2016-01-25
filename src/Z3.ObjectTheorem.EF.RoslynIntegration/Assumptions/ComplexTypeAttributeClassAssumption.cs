using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Z3.ObjectTheorem.EF.Metamodel;

namespace Z3.ObjectTheorem.EF.RoslynIntegration.Assumptions
{
    public class ComplexTypeAttributeClassAssumption : ClassAssumption<ReferenceType, bool>
    {
        public ComplexTypeAttributeClassAssumption(INamedTypeSymbol @class, Expression<Func<bool>> assumption, ReferenceType refType)
            : base(@class, assumption, refType)
        { }

        public override Expression<Func<ReferenceType, bool>> PropertyExpression => f => f.HasComplexTypeAttribute;

        public override int Rank
        {
            get
            {
                return 6;
            }
        }

        protected override IEnumerable<string> GetAttributesToAdd(bool result)
        {
            if (result)
            {
                yield return "ComplexType";
            }
        }

        protected override IEnumerable<string> GetAttributesToDelete(bool result)
        {
            if (!result)
            {
                yield return "ComplexType";
            }
        }
    }
}