using Microsoft.Z3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Z3.ObjectTheorem.Helper;
using Z3.ObjectTheorem.Solving.Helper;

namespace Z3.ObjectTheorem.Solving
{
    internal class LambdaExpressionToConstraintGenerator
    {
        private readonly Context _context;
        private readonly Environment _environment;

        private readonly Dictionary<MemberExpression, Expr> _memberAccessExpressions = new Dictionary<MemberExpression, Expr>();

        internal LambdaExpressionToConstraintGenerator(Context context, Environment environment)
        {
            _context = context;
            _environment = environment;
        }

        internal Dictionary<string, Expr> LambdaParameterConstants { get; set; }

        internal BoolExpr Visit(LambdaExpression lambdaExpression)
        {
            return (BoolExpr)Visit(lambdaExpression.Body, lambdaExpression.Parameters);
        }

        internal static FuncDecl GetOrAddMemberAccessFunction(Context context, Environment environment, MemberInfo memberInfo)
        {
            FuncDecl memberFunc;
            if (!environment.Members.TryGetValue(memberInfo, out memberFunc))
            {
                Sort memberTypeSort;
                if (!environment.Types.TryGetValue(memberInfo.DeclaringType, out memberTypeSort))
                {
                    throw new KeyNotFoundException(memberInfo.DeclaringType + " could not be found at environment.Types");
                }

                Sort memberReturnTypeEnumSort;
                var propertyType = ((PropertyInfo)memberInfo).PropertyType;
                if (propertyType == typeof(bool))
                    memberReturnTypeEnumSort = context.MkBoolSort();
                else if (propertyType == typeof(int))
                    memberReturnTypeEnumSort = context.MkIntSort();
                else if (propertyType == typeof(long))
                    memberReturnTypeEnumSort = context.MkRealSort();
                else if (propertyType == typeof(string))
                    memberReturnTypeEnumSort = environment.PossibleStringValues;
                else
                {
                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        var listItemType = propertyType.GenericTypeArguments[0];

                        var listSort = context.MkSetSort(environment.Types[listItemType]);

                        memberReturnTypeEnumSort = listSort;

                        // TODO: add TryGetValue
                        environment.Types.Add(propertyType, listSort);
                    }
                    else if (propertyType.IsEnum)
                    {
                        EnumSort enumSort = context.MkEnumSort(propertyType.Name, Enum.GetNames(propertyType));

                        memberReturnTypeEnumSort = enumSort;

                        // TODO: add TryGetValue
                        environment.Types.Add(propertyType, enumSort);
                    }
                    else
                    {
                        // TODO throw exception if type is not supported
                        memberReturnTypeEnumSort = environment.Types[propertyType];
                    }
                }

                memberFunc = context.MkFuncDecl(memberInfo.Name, memberTypeSort, memberReturnTypeEnumSort);
                environment.Members.Add(memberInfo, memberFunc);
            }
            return memberFunc;
        }

        private Expr Visit(Expression expression, IReadOnlyList<ParameterExpression> parameters)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return VisitBinary((BinaryExpression)expression, parameters, (a, b) => _context.MkAdd((ArithExpr)a, (ArithExpr)b));

                case ExpressionType.AddAssign:
                    break;

                case ExpressionType.AddAssignChecked:
                    break;

                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return VisitBinary((BinaryExpression)expression, parameters, (a, b) => _context.MkAnd((BoolExpr)a, (BoolExpr)b));

                case ExpressionType.ArrayIndex:
                    break;

                case ExpressionType.ArrayLength:
                    break;

                case ExpressionType.Assign:
                    break;

                case ExpressionType.Block:
                    break;

                case ExpressionType.Call:
                    return VisitCall((MethodCallExpression)expression, parameters);

                case ExpressionType.Coalesce:
                    break;

                case ExpressionType.Conditional:
                    break;

                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)expression);

                case ExpressionType.Convert:
                    var convertExpression = (UnaryExpression)expression;
                    if (convertExpression.Operand.Type.IsEnum &&
                        convertExpression.Operand.NodeType == ExpressionType.MemberAccess)
                    {
                        return VisitMember((MemberExpression)convertExpression.Operand, parameters);
                    }
                    break;

                case ExpressionType.ConvertChecked:
                    break;

                case ExpressionType.DebugInfo:
                    break;

                case ExpressionType.Decrement:
                    break;

                case ExpressionType.Default:
                    break;

                case ExpressionType.Divide:
                    return VisitBinary((BinaryExpression)expression, parameters, (a, b) => _context.MkDiv((ArithExpr)a, (ArithExpr)b));

                case ExpressionType.DivideAssign:
                    break;

                case ExpressionType.Dynamic:
                    break;

                case ExpressionType.Equal:
                    return VisitBinary((BinaryExpression)expression, parameters, (a, b) => _context.MkEq(a, b));

                case ExpressionType.ExclusiveOr:
                    return VisitBinary((BinaryExpression)expression, parameters, (a, b) => _context.MkXor((BoolExpr)a, (BoolExpr)b));

                case ExpressionType.Extension:
                    break;

                case ExpressionType.Goto:
                    break;

                case ExpressionType.GreaterThan:
                    return VisitBinary((BinaryExpression)expression, parameters, (a, b) => _context.MkGt((ArithExpr)a, (ArithExpr)b));

                case ExpressionType.GreaterThanOrEqual:
                    return VisitBinary((BinaryExpression)expression, parameters, (a, b) => _context.MkGe((ArithExpr)a, (ArithExpr)b));

                case ExpressionType.Increment:
                    break;

                case ExpressionType.Index:
                    break;

                case ExpressionType.Invoke:
                    break;

                case ExpressionType.IsFalse:
                    break;

                case ExpressionType.IsTrue:
                    break;

                case ExpressionType.Label:
                    break;

                case ExpressionType.Lambda:
                    break;

                case ExpressionType.LeftShift:
                    break;

                case ExpressionType.LeftShiftAssign:
                    break;

                case ExpressionType.LessThan:
                    return VisitBinary((BinaryExpression)expression, parameters, (a, b) => _context.MkLt((ArithExpr)a, (ArithExpr)b));

                case ExpressionType.LessThanOrEqual:
                    return VisitBinary((BinaryExpression)expression, parameters, (a, b) => _context.MkLe((ArithExpr)a, (ArithExpr)b));

                case ExpressionType.ListInit:
                    break;

                case ExpressionType.Loop:
                    break;

                case ExpressionType.MemberAccess:
                    return VisitMember((MemberExpression)expression, parameters);

                case ExpressionType.MemberInit:
                    break;

                case ExpressionType.Modulo:
                    return VisitBinary((BinaryExpression)expression, parameters, (a, b) => _context.MkRem((IntExpr)a, (IntExpr)b));

                case ExpressionType.ModuloAssign:
                    break;

                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return VisitBinary((BinaryExpression)expression, parameters, (a, b) => _context.MkMul((ArithExpr)a, (ArithExpr)b));

                case ExpressionType.MultiplyAssign:
                    break;

                case ExpressionType.MultiplyAssignChecked:
                    break;

                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    return VisitUnary((UnaryExpression)expression, parameters, (a) => _context.MkUnaryMinus((ArithExpr)a));

                case ExpressionType.New:
                    break;

                case ExpressionType.NewArrayBounds:
                    return VisitNewArray((NewArrayExpression)expression, parameters);

                case ExpressionType.NewArrayInit:
                    return VisitNewArray((NewArrayExpression)expression, parameters);

                case ExpressionType.Not:
                    return VisitUnary((UnaryExpression)expression, parameters, (a) => _context.MkNot((BoolExpr)a));

                case ExpressionType.NotEqual:
                    return VisitBinary((BinaryExpression)expression, parameters, (a, b) => _context.MkNot(_context.MkEq(a, b)));

                case ExpressionType.OnesComplement:
                    break;

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return VisitBinary((BinaryExpression)expression, parameters, (a, b) => _context.MkOr((BoolExpr)a, (BoolExpr)b));

                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)expression, parameters);

                case ExpressionType.PostDecrementAssign:
                    break;

                case ExpressionType.PostIncrementAssign:
                    break;

                case ExpressionType.Power:
                    break;

                case ExpressionType.PowerAssign:
                    break;

                case ExpressionType.PreDecrementAssign:
                    break;

                case ExpressionType.PreIncrementAssign:
                    break;

                case ExpressionType.Quote:
                    break;

                case ExpressionType.RightShift:
                    break;

                case ExpressionType.RightShiftAssign:
                    break;

                case ExpressionType.RuntimeVariables:
                    break;

                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return VisitBinary((BinaryExpression)expression, parameters, (a, b) => _context.MkSub((ArithExpr)a, (ArithExpr)b));

                case ExpressionType.SubtractAssign:
                    break;

                case ExpressionType.SubtractAssignChecked:
                    break;

                case ExpressionType.Switch:
                    break;

                case ExpressionType.Throw:
                    break;

                case ExpressionType.Try:
                    break;

                case ExpressionType.TypeAs:
                    break;

                case ExpressionType.TypeEqual:
                    break;

                case ExpressionType.TypeIs:
                    break;

                case ExpressionType.UnaryPlus:
                    break;

                case ExpressionType.Unbox:
                    break;

                default:
                    break;
            }

            throw new NotSupportedException("Unsupported expression node type encountered: " + expression.NodeType);
        }

        private Expr VisitParameter(ParameterExpression parameterExpression, IReadOnlyList<ParameterExpression> parameters)
        {
            return LambdaParameterConstants[parameterExpression.Name];
        }

        private Expr VisitNewArray(NewArrayExpression newArrayExpression, IReadOnlyList<ParameterExpression> parameters)
        {
            var itemType = newArrayExpression.Type.GetElementType();
            var set = _context.MkEmptySet(_environment.Types[itemType]);

            foreach (MemberExpression expression in newArrayExpression.Expressions.OfType<MemberExpression>())
            {
                var element = VisitMember(expression, parameters);
                set = _context.MkSetAdd(set, element);
            }

            return set;
        }

        private Expr VisitCall(MethodCallExpression methodCallExpression, IReadOnlyList<ParameterExpression> parameters)
        {
            var method = methodCallExpression.Method;

            // TODO: Distinct
            //var args = from arg in arr.Expressions select Visit(context, environment, arg, param);
            //return context.MkDistinct(args.ToArray());

            // bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate);
            if (method.IsGenericMethod && method.GetGenericMethodDefinition() == typeof(Enumerable).GetMethods().Single(m => m.Name == "Any" && m.GetParameters().Length == 2))
            {
                var source = (MemberExpression)methodCallExpression.Arguments[0];
                var predicate = (LambdaExpression)methodCallExpression.Arguments[1];

                var sourceExpr = VisitMember(source, parameters);

                var sourceExistConst = _context.MkConst("Any", ((ArraySort)sourceExpr.Sort).Domain);

                LambdaParameterConstants.Add(predicate.Parameters[0].Name, sourceExistConst);

                var predicateExpr = (BoolExpr)Visit(predicate.Body, parameters.Concat(predicate.Parameters).ToList());

                var existsExpr = _context.MkExists(new[] { sourceExistConst },
                    _context.MkAnd((BoolExpr)_context.MkSetMembership(sourceExistConst, sourceExpr), predicateExpr)
                );

                return existsExpr;

            }

            // bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate);
            if (method.IsGenericMethod && method.GetGenericMethodDefinition() == typeof(Enumerable).GetMethod("All"))
            {
                var source = (MemberExpression)methodCallExpression.Arguments[0];
                var predicate = (LambdaExpression)methodCallExpression.Arguments[1];

                var sourceExpr = VisitMember(source, parameters);

                var sourceExistConst = _context.MkConst("All", ((ArraySort)sourceExpr.Sort).Domain);

                LambdaParameterConstants.Add(predicate.Parameters[0].Name, sourceExistConst);

                var predicateExpr = (BoolExpr)Visit(predicate.Body, parameters.Concat(predicate.Parameters).ToList());

                var forAllExpr = _context.MkForall(new[] { sourceExistConst },
                    _context.MkOr(_context.MkNot((BoolExpr)_context.MkSetMembership(sourceExistConst, sourceExpr)), predicateExpr)
                );

                return forAllExpr;
            }

            throw new NotSupportedException("Unknown method call:" + method.ToString());
        }

        private Expr VisitBinary(BinaryExpression expression, IReadOnlyList<ParameterExpression> parameters, Func<Expr, Expr, Expr> ctor)
        {
            if (expression.Left.NodeType == ExpressionType.Convert &&
                ((UnaryExpression)expression.Left).Operand.Type.IsEnum &&
                IsConstant(expression.Right))
            {
                var leftExpr = Visit(expression.Left, parameters);

                var simplifiedConstant = (ConstantExpression)new ConstantExpressionSimplifier().Visit(expression.Right);

                var rightExpr = VisitConstant(simplifiedConstant, enumType: ((UnaryExpression)expression.Left).Operand.Type);

                return ctor(leftExpr, rightExpr);
            }

            if (expression.Right.NodeType == ExpressionType.Convert &&
                ((UnaryExpression)expression.Right).Operand.Type.IsEnum &&
                IsConstant(expression.Left))
            {
                var simplifiedConstant = (ConstantExpression)new ConstantExpressionSimplifier().Visit(expression.Left);

                var leftExpr = VisitConstant(simplifiedConstant, enumType: ((UnaryExpression)expression.Right).Operand.Type);

                var rightExpr = Visit(expression.Right, parameters);

                return ctor(leftExpr, rightExpr);
            }

            if (expression.Left.NodeType == ExpressionType.Constant &&
                expression.Left.Type == typeof(string))
            {
                var leftExpr = VisitConstant((ConstantExpression)expression.Left, stringMemberInfo: ((MemberExpression)expression.Right).Member);

                var rightExpr = Visit(expression.Right, parameters);

                return ctor(leftExpr, rightExpr);
            }

            if (expression.Right.NodeType == ExpressionType.Constant &&
                expression.Right.Type == typeof(string))
            {
                var leftExpr = Visit(expression.Left, parameters);

                var rightExpr = VisitConstant((ConstantExpression)expression.Right, stringMemberInfo: ((MemberExpression)expression.Left).Member);

                return ctor(leftExpr, rightExpr);
            }

            if (expression.Right.NodeType == ExpressionType.MemberAccess &&
                expression.Right.Type == typeof(string))
            {
                var fieldExpression = ((MemberExpression)expression.Right);

                if (fieldExpression.Member.MemberType == MemberTypes.Field && IsConstant(fieldExpression.Expression))
                {
                    var leftExpr = Visit(expression.Left, parameters);

                    // TODO: use ConstantExpressionSimplifier based on types (all theorem types are not used for constants)
                    var simplifiedConstant = (ConstantExpression)new ConstantExpressionSimplifier().Visit(fieldExpression);

                    var rightExpr = VisitConstant(simplifiedConstant, stringMemberInfo: ((MemberExpression)expression.Left).Member);

                    return ctor(leftExpr, rightExpr);
                }
            }

            return ctor(Visit(expression.Left, parameters), Visit(expression.Right, parameters));
        }

        bool IsConstant(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant)
            {
                return true;
            }

            if (expression.NodeType == ExpressionType.MemberAccess &&
                (expression.Type.IsEnum || expression.Type.GetCustomAttribute<CompilerGeneratedAttribute>() != null))
            {
                return IsConstant(((MemberExpression)expression).Expression);
            }

            if (expression.NodeType == ExpressionType.Convert)
            {
                return IsConstant(((UnaryExpression)expression).Operand);
            }

            return false;
        }

        private Expr VisitConstant(ConstantExpression constant, Type enumType = null, MemberInfo stringMemberInfo = null)
        {
            if (enumType != null)
            {
                var enumValueName = Enum.GetName(enumType, constant.Value);
                EnumSort enumSort = (EnumSort)_environment.Types[enumType];
                return enumSort.Consts.Single(c => c.FuncDecl.Name.ToString() == enumValueName);
            }

            if (stringMemberInfo != null)
            {
                EnumSort enumSort = (EnumSort)_environment.PossibleStringValues;
                return enumSort.Consts.Single(c => c.FuncDecl.Name.ToString().Equals(constant.Value));
            }

            if (constant.Type.IsArray)
            {
                var itemType = constant.Type.GetElementType();
                var set = _context.MkEmptySet(_environment.Types[itemType]);

                foreach (object item in (IEnumerable)constant.Value)
                {
                    InstanceInfo outsideMemberInfo = _environment.Instances.Values.SingleOrDefault(i => i.ObjectInstance == item);
                    var element = outsideMemberInfo.EnumConstant;
                    set = _context.MkSetAdd(set, element);
                }

                return set;
            }

            if (constant.Type == typeof(bool))
                return (bool)constant.Value ? _context.MkTrue() : _context.MkFalse();

            if (constant.Type == typeof(int))
                return _context.MkNumeral((int)constant.Value, _context.IntSort);

            if (constant.Type == typeof(long))
                return _context.MkNumeral((long)constant.Value, _context.RealSort);

            throw new NotSupportedException("Unsupported constant type.");
        }

        private Expr VisitMember(MemberExpression memberExpression, IReadOnlyList<ParameterExpression> parameters)
        {
            Expr memberAccessExpression;
            if (_memberAccessExpressions.TryGetValue(memberExpression, out memberAccessExpression))
            {
                return memberAccessExpression;
            }

            //
            // E.g. theorem.AssumeAll<ClassA, ClassB>((a, b) => a.IsValidA == true)
            //                                                  ^^
            if (parameters.Any(p => p == memberExpression.Expression))
            {
                Expr memberAccessParameter = VisitParameter((ParameterExpression)memberExpression.Expression, parameters);

                return AccessMember(memberAccessParameter, memberExpression.Member);
            }
            else
            {
                object outsideLambdaMemberValue = null;

                if (IsConstant(memberExpression.Expression)) 
                {
                    var simplifiedConstant = (ConstantExpression)new ConstantExpressionSimplifier().Visit(memberExpression);

                    // var outsideBoolConstant = true;
                    // ...
                    //     .Assert(() => classA.IsValidA == outsideBoolConstant)
                    //                                      ^^
                    if (simplifiedConstant.Type.IsPrimitive ||
                        simplifiedConstant.Type == typeof(string))
                    {
                        return VisitConstant(simplifiedConstant);
                    }
                    else if (simplifiedConstant.Type.IsArray)
                    {
                        // TODO: make decition based on theorem types
                        return VisitConstant(simplifiedConstant);
                    }
                    // var typeInstance1 = objectTheorem.CreateInstance<Type>("TypeInstance1");
                    // ...
                    //     .Assert(() => fieldInstance1.Type == typeInstance1)
                    //                                          ^^
                    else
                    {
                        outsideLambdaMemberValue = simplifiedConstant.Value;
                    }
                }
                else if (memberExpression.Expression.NodeType == ExpressionType.MemberAccess)
                {
                    var outsideLambdaMember = (MemberExpression)memberExpression.Expression;

                    //     .Assert(() => type1Instance.IsEntity == true)
                    //                   ^^
                    if (outsideLambdaMember.Expression.NodeType == ExpressionType.Constant)
                    {
                        outsideLambdaMemberValue = GetConstantValue(outsideLambdaMember);
                    }
                }
                else
                {
                    throw new NotSupportedException("Unknown parameter encountered: " + memberExpression.Member.Name + ".");
                }

                Expr objectToForMemberAccess = null;

                if (outsideLambdaMemberValue != null)
                {
                    InstanceInfo outsideMemberInfo = _environment.Instances.Values.SingleOrDefault(i => i.ObjectInstance == outsideLambdaMemberValue);
                    if (outsideMemberInfo != null)
                    {
                        objectToForMemberAccess = outsideMemberInfo.EnumConstant;
                    }
                }
                //     .Assert(() => fieldInstance1.Type.IsEntity == true)
                //                                  ^^
                else
                {
                    objectToForMemberAccess = VisitMember((MemberExpression)memberExpression.Expression, parameters);
                }

                if (IsConstant(memberExpression.Expression))
                {
                    return objectToForMemberAccess;
                }

                return AccessMember(objectToForMemberAccess, memberExpression.Member);
            }

            throw new NotSupportedException("Unknown parameter encountered: " + memberExpression.Member.Name + ".");
        }

        private Expr AccessMember(Expr objectForMemberAccess, MemberInfo memberInfo)
        {
            FuncDecl memberFunc = GetOrAddMemberAccessFunction(_context, _environment, memberInfo);

            Sort memberDeclaredObjectSort = memberFunc.Domain[0];
            if (memberDeclaredObjectSort != objectForMemberAccess.Sort)
            {
                objectForMemberAccess = UpcastHelper.Upcast(_context, objectForMemberAccess, memberDeclaredObjectSort);
            }

            return _context.MkApp(memberFunc, objectForMemberAccess);
        }

        private static object GetConstantValue(MemberExpression memberExpressionWithConstantExpression)
        {
            var ce = (ConstantExpression)memberExpressionWithConstantExpression.Expression;
            var fieldInfo = ce.Value
                .GetType()
                .GetField(memberExpressionWithConstantExpression.Member.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return fieldInfo.GetValue(ce.Value);
        }

        private Expr VisitUnary(UnaryExpression expression, IReadOnlyList<ParameterExpression> parameters, Func<Expr, Expr> ctor)
        {
            return ctor(Visit(expression.Operand, parameters));
        }
    }
}