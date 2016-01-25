using Microsoft.Z3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Z3.ObjectTheorem.Solving.Helper;

namespace Z3.ObjectTheorem.Solving
{
    public class ObjectTheoremResult
    {
        private readonly Microsoft.Z3.Context _context;
        private readonly Environment _environment;
        private readonly List<LambdaExpression> _failedAssumptions = new List<LambdaExpression>();
        private readonly Solver _solver;

        internal ObjectTheoremResult(Context context, Environment environment, Solver solver, Microsoft.Z3.Status status)
        {
            _context = context;
            _environment = environment;
            _solver = solver;
            Status = (Status)status;
        }

        public Status Status { get; private set; }

        public TProperty GetValue<TObject, TProperty>(TObject objectToInspect, Expression<Func<TObject, TProperty>> propertyExpression) where TObject : class
        {
            try
            {
                var memberExpression = (MemberExpression)propertyExpression.Body;

                FuncDecl member = _environment.Members[memberExpression.Member];
                InstanceInfo instance = _environment.Instances.Select(i => i.Value).SingleOrDefault(i => i.ObjectInstance == objectToInspect);
                Expr objectForMemberAccess = instance.EnumConstant;

                Sort memberDeclaredObjectSort = member.Domain[0];
                if (memberDeclaredObjectSort != objectForMemberAccess.Sort)
                {
                    objectForMemberAccess = UpcastHelper.Upcast(_context, objectForMemberAccess, memberDeclaredObjectSort);
                }

                var memberOfInstance = member.Apply(objectForMemberAccess);
                var result = _solver.Model.Evaluate(memberOfInstance);

                if (typeof(TProperty) == typeof(bool))
                {
                    if (result.IsBool)
                    {
                        return (TProperty)(object)result.IsTrue;
                    }
                }
                if (typeof(TProperty) == typeof(int))
                {
                    if (result.IsInt)
                    {
                        return (TProperty)(object)((IntNum)result).Int;
                    }
                }
                if (typeof(TProperty) == typeof(string))
                {
                    return (TProperty)(object)result.FuncDecl.Name.ToString();
                }
                if (typeof(TProperty).IsEnum)
                {
                    return (TProperty)Enum.Parse(typeof(TProperty), result.FuncDecl.Name.ToString());
                }
                if (typeof(TProperty).IsGenericType
                    && typeof(TProperty).GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    var listInstance = (IList)typeof(List<>)
                      .MakeGenericType(typeof(TProperty).GetGenericArguments()[0])
                      .GetConstructor(Type.EmptyTypes)
                      .Invoke(null);

                    var storeExpr = result;
                    if (storeExpr.Args.Length == 3)
                    {
                        while (storeExpr.Args.Length == 3)
                        {
                            listInstance.Add(_environment.Instances.Values.Single(i => i.EnumConstant == storeExpr.Args[1]).ObjectInstance);
                            storeExpr = storeExpr.Args[0];
                        }
                    }
                    else if (storeExpr.Args.Length == 1 &&
                        storeExpr.Args[0].IsBool && !storeExpr.Args[0].IsTrue)
                    {
                        // Empty Array
                    }
                    else
                    {
                        var arrayInterpretationFuncDecl = result.FuncDecl.Parameters[0].FuncDecl;
                        var arryItemSort = (DatatypeSort)arrayInterpretationFuncDecl.Domain[0];
                        foreach (var possibleItem in _environment.Instances.Values
                            .Where(i => i.EnumConstant.Sort == arryItemSort))
                        {
                            if (_solver.Model.Evaluate(arrayInterpretationFuncDecl.Apply(possibleItem.EnumConstant)).IsTrue)
                            {
                                listInstance.Add(possibleItem.ObjectInstance);
                            }
                        }
                    }

                    return (TProperty)listInstance;
                }
                if (typeof(TProperty).IsClass)
                {
                    return (TProperty)_environment.Instances.Single(i => i.Value.EnumConstant == result).Value.ObjectInstance;
                }

                throw new NotSupportedException(typeof(TProperty).Name);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                throw;
            }
        }

        public bool IsAssumptionWrong(LambdaExpression assumption)
        {
            return _failedAssumptions.Contains(assumption);
        }

        internal void SetUnsatCore(Dictionary<LambdaExpression, BoolExpr> assumptions, Expr[] expr)
        {
            foreach (var failedAssumption in assumptions.Where(a => expr.Contains(a.Value)))
            {
                _failedAssumptions.Add(failedAssumption.Key);
            }
        }
    }
}