using System.Linq.Expressions;
using System.Reflection;

namespace Z3.ObjectTheorem.Helper
{
    // http://stackoverflow.com/questions/6998523/how-to-get-the-value-of-a-constantexpression-which-uses-a-local-variable
    internal class ConstantExpressionSimplifier : ExpressionVisitor
    {
        protected override Expression VisitMember
            (MemberExpression memberExpression)
        {
            // Recurse down to see if we can simplify...
            var expression = Visit(memberExpression.Expression);

            // If we've ended up with a constant, and it's a property or a field,
            // we can simplify ourselves to a constant
            if (expression is ConstantExpression)
            {
                object container = ((ConstantExpression)expression).Value;
                var member = memberExpression.Member;
                if (member is FieldInfo)
                {
                    object value = ((FieldInfo)member).GetValue(container);
                    return Expression.Constant(value);
                }
                if (member is PropertyInfo)
                {
                    object value = ((PropertyInfo)member).GetValue(container, null);
                    return Expression.Constant(value);
                }
            }
            return base.VisitMember(memberExpression);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Convert)
            {
                return base.Visit(node.Operand);
            }

            return base.VisitUnary(node);
        }
    }
}