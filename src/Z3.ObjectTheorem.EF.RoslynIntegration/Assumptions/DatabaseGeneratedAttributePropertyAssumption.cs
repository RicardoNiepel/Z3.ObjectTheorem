using Microsoft.CodeAnalysis;
using System;
using System.Linq.Expressions;
using Z3.ObjectTheorem.EF.Metamodel;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Z3.ObjectTheorem.EF.RoslynIntegration.Assumptions
{
    public class DatabaseGeneratedAttributePropertyAssumption : PropertyAssumption<ValueField, DatabaseGeneratedAttribute>
    {
        public DatabaseGeneratedAttributePropertyAssumption(IPropertySymbol property, Expression<Func<bool>> assumption, ValueField valueField)
            : base(property, assumption, valueField)
        { }

        public override Expression<Func<ValueField, DatabaseGeneratedAttribute>> PropertyExpression => vf => vf.DatabaseGeneratedAttribute;

        public override int Rank
        {
            get
            {
                return 2;
            }
        }

        protected override IEnumerable<AttributeSyntax> GetAttributeSyntaxesToAdd(DatabaseGeneratedAttribute result)
        {
            if (result != DatabaseGeneratedAttribute.None)
            {
                yield return SyntaxFactory.Attribute(
                    SyntaxFactory.IdentifierName("DatabaseGenerated"),
                    SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.AttributeArgument(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName(
                                        "DatabaseGeneratedOption"),
                                    SyntaxFactory.IdentifierName(
                                        result.ToString()))
                            )
                        )
                    )
                );
            }
        }

        protected override IEnumerable<string> GetAttributesToDelete(DatabaseGeneratedAttribute result)
        {
            if (result == DatabaseGeneratedAttribute.None)
            {
                yield return "DatabaseGenerated";
            }
        }
    }
}
