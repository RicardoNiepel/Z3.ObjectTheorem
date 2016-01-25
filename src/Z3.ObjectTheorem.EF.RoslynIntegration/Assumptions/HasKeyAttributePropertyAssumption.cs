using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Z3.ObjectTheorem.EF.Metamodel;

namespace Z3.ObjectTheorem.EF.RoslynIntegration.Assumptions
{
    public class HasKeyAttributePropertyAssumption : PropertyAssumption<ValueField, bool>
    {
        public HasKeyAttributePropertyAssumption(IPropertySymbol property, Expression<Func<bool>> assumption, ValueField valueField)
            : base(property, assumption, valueField)
        { }

        public override Expression<Func<ValueField, bool>> PropertyExpression => vf => vf.HasKeyAttribute;

        public override int Rank { get; } = 1;

        protected override IEnumerable<string> GetAttributesToAdd(bool result)
        {
            if (result)
            {
                yield return "Key";
            }
        }

        protected override IEnumerable<string> GetAttributesToDelete(bool result)
        {
            if (!result)
            {
                yield return "Key";
            }
        }
    }
}