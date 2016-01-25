using Microsoft.CodeAnalysis;
using System;
using System.Linq.Expressions;
using Z3.ObjectTheorem.EF.Metamodel;
using System.Collections.Generic;

namespace Z3.ObjectTheorem.EF.RoslynIntegration.Assumptions
{
    public class HasForeignKeyAttributePropertyAssumption : PropertyAssumption<SingleField, string>
    {
        public HasForeignKeyAttributePropertyAssumption(IPropertySymbol property, Expression<Func<bool>> assumption, SingleField singleField)
            : base(property, assumption, singleField)
        { }

        public override Expression<Func<SingleField, string>> PropertyExpression => sf => sf.ForeignKeyAttribute;

        public override int Rank
        {
            get
            {
                return 3;
            }
        }

        protected override IEnumerable<string> GetAttributesToAdd(string result)
        {
            if (result != "-")
            {
                yield return "ForeignKey";
            }
        }

        protected override IEnumerable<string> GetAttributesToDelete(string result)
        {
            if (result == "-")
            {
                yield return "ForeignKey";
            }
        }
    }
}
