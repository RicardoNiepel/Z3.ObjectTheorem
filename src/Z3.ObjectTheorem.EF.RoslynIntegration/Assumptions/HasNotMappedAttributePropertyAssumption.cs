using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Z3.ObjectTheorem.EF.Metamodel;

namespace Z3.ObjectTheorem.EF.RoslynIntegration.Assumptions
{
    public class HasNotMappedAttributePropertyAssumption : PropertyAssumption<Field, bool>
    {
        public HasNotMappedAttributePropertyAssumption(IPropertySymbol property, Expression<Func<bool>> assumption, Field valueField)
            : base(property, assumption, valueField)
        { }

        public override Expression<Func<Field, bool>> PropertyExpression => f => f.HasNotMappedAttribute;

        public override int Rank
        {
            get
            {
                return 7;
            }
        }

        protected override IEnumerable<string> GetAttributesToAdd(bool result)
        {
            if (result)
            {
                yield return "NotMapped";
            }
        }

        protected override IEnumerable<string> GetAttributesToDelete(bool result)
        {
            if (!result)
            {
                yield return "NotMapped";
            }
        }
    }
}
