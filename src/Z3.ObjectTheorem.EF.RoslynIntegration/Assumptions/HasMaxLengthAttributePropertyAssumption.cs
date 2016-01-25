using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Z3.ObjectTheorem.EF.Metamodel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Z3.ObjectTheorem.EF.RoslynIntegration.Assumptions
{
    public class HasMaxLengthAttributePropertyAssumption : PropertyAssumption<ValueField, bool>
    {
        public HasMaxLengthAttributePropertyAssumption(IPropertySymbol property, Expression<Func<bool>> assumption, ValueField valueField)
            : base(property, assumption, valueField)
        { }

        public override Expression<Func<ValueField, bool>> PropertyExpression => vf => vf.HasMaxLengthAttribute;

        public override int Rank
        {
            get
            {
                return 3;
            }
        }

        protected override IEnumerable<string> GetAttributesToDelete(bool result)
        {
            if (!result)
            {
                yield return "MaxLength";
            }
        }

        protected override IEnumerable<AttributeSyntax> GetAttributeSyntaxesToAdd(bool result)
        {
            if (result)
            {
                yield return SyntaxFactory.Attribute(
                    SyntaxFactory.IdentifierName("MaxLength"),
                    SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.AttributeArgument(
                                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(255))
                            )
                        )
                    )
                );
            }
        }
    }
}
