using Microsoft.Z3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Z3.ObjectTheorem.Solving
{
    internal class ObjectTheoremSolver
    {
        private readonly IReadOnlyList<LambdaExpression> _allAssertions;
        private readonly IReadOnlyList<LambdaExpression> _allAssumptions;
        private readonly IReadOnlyList<LambdaExpression> _assertions;
        private readonly IReadOnlyList<LambdaExpression> _assumptions;
        private readonly IReadOnlyDictionary<string, object> _instances;
        private readonly IReadOnlyList<MemberInfo> _members;
        private readonly IReadOnlyList<string> _possibleStringValues;
        private readonly IReadOnlyList<Type> _superTypes;

        internal ObjectTheoremSolver(
            IReadOnlyDictionary<string, object> instances, IReadOnlyList<Type> superTypes,
            IReadOnlyList<string> possibleStringValues, IReadOnlyList<MemberInfo> members,
            IReadOnlyList<LambdaExpression> assertions, IReadOnlyList<LambdaExpression> assumptions,
            IReadOnlyList<LambdaExpression> allAssertions, IReadOnlyList<LambdaExpression> allAssumptions)
        {
            _instances = instances;
            _superTypes = superTypes;
            _possibleStringValues = possibleStringValues;
            _members = members;

            _assertions = assertions;
            _assumptions = assumptions;
            _allAssertions = allAssertions;
            _allAssumptions = allAssumptions;
        }

        internal ObjectTheoremResult Solve()
        {
            var settings = new Dictionary<string, string> {
                { "model", "true" },
                { "unsat_core", "true" }
            };

            using (var context = new Context(settings))
            {
                var stopwatch = Stopwatch.StartNew();

                Environment environment = GenerateEnvironment(context);

                BoolExpr[] assertions = GenerateConstraints(context, environment, _assertions);

                BoolExpr[] allAssertions = GenerateAllConstraints(context, environment, _allAssertions);

                Solver solver = context.MkSimpleSolver();

                int assertionCount = 0;
                foreach (var assertion in assertions.Concat(allAssertions))
                {
                    assertionCount++;
                    solver.Assert(assertion);
                }

                int assumptionCount = 0;
                var assumptions = new Dictionary<LambdaExpression, BoolExpr>();
                foreach (var assumption in _assumptions)
                {
                    var generator = new LambdaExpressionToConstraintGenerator(context, environment);

                    var assumptionExpr = generator.Visit(assumption);

                    var assumptionCheck = context.MkBoolConst(Guid.NewGuid().ToString());
                    solver.Assert(context.MkEq(assumptionExpr, assumptionCheck));

                    assumptionCount++;
                    assumptions.Add(assumption, assumptionCheck);
                }

                stopwatch.Stop();
                var constraintGenerationTimeSpan = stopwatch.Elapsed;

                var solverString = solver.ToString();
                var totalConstraintsCount = solverString.Split('\n').Count(l => l.StartsWith("  ("));

                Trace.WriteLine("Statistics:");
                Trace.WriteLine("assertionCount: " + assertionCount);
                Trace.WriteLine("assumptionCount: " + assumptionCount);
                Trace.WriteLine("totalConstraintsCount: " + totalConstraintsCount);
                Trace.WriteLine("constraintGenerationTimeSpan: " + constraintGenerationTimeSpan);

                var classCount = environment.Types.Count(t => !t.Key.IsValueType && t.Key.Name != "String");
                var propertyCount = environment.Types
                    .Where(t => !t.Key.IsValueType && t.Key.Name != "String")
                    .Select(p => p.Key.GetProperties())
                    .Count();

                stopwatch.Restart();
                Microsoft.Z3.Status status = solver.Check(assumptions.Values.ToArray());
                stopwatch.Stop();

                var solvingTimeSpan = stopwatch.Elapsed;
                Trace.WriteLine("solvingTimeSpan: " + solvingTimeSpan);

                if (status != Microsoft.Z3.Status.SATISFIABLE)
                {
                    if (solver.UnsatCore.Length > 0)
                    {
                        var result = new ObjectTheoremResult(context, environment, solver, status);
                        result.SetUnsatCore(assumptions, solver.UnsatCore);
                        return result;
                    }

                    return null;
                }

                var modelString = solver.Model.ToString();

                return new ObjectTheoremResult(context, environment, solver, status);
            }
        }

        private BoolExpr[] GenerateAllConstraints(Context context, Environment environment, IReadOnlyList<LambdaExpression> lambdaExpressions)
        {
            var constraints = new List<BoolExpr>();

            foreach (LambdaExpression lambdaExpression in lambdaExpressions)
            {
                var forAllParameters = new Dictionary<string, Expr>();
                foreach (ParameterExpression parameter in lambdaExpression.Parameters)
                {
                    Sort typeSort;
                    if (!environment.Types.TryGetValue(parameter.Type, out typeSort))
                    {
                        throw new NotSupportedException("This type was not registered through objectTheorem.CreateInstance<T> :" + parameter.Type);
                    }

                    forAllParameters.Add(parameter.Name, context.MkConst(parameter.Type.ToString(), typeSort));
                }

                var generator = new LambdaExpressionToConstraintGenerator(context, environment);
                generator.LambdaParameterConstants = forAllParameters;
                Expr forAllBody = generator.Visit(lambdaExpression);

                var forAll = context.MkForall(forAllParameters.Values.ToArray(), forAllBody);

                constraints.Add(forAll);
            }

            return constraints.ToArray();
        }

        private BoolExpr[] GenerateConstraints(Context context, Environment environment, IReadOnlyList<LambdaExpression> lambdaExpressions)
        {
            var constraints = new List<BoolExpr>();

            foreach (var lambdaExpression in lambdaExpressions)
            {
                var generator = new LambdaExpressionToConstraintGenerator(context, environment);
                constraints.Add(generator.Visit(lambdaExpression));
            }

            return constraints.ToArray();
        }

        private Environment GenerateEnvironment(Context context)
        {
            var environment = new Environment();

            // Class instances
            // TODO: support NULL values
            foreach (var instancesPerClassType in _instances.GroupBy(_ => _.Value.GetType()))
            {
                string classType = instancesPerClassType.Key.Name;

                EnumSort instancesEnumSort = context.MkEnumSort(classType + "_instances", instancesPerClassType.Select(_ => _.Key).ToArray());
                environment.Types.Add(instancesPerClassType.Key, instancesEnumSort);

                Expr[] instancesEnumSortValues = instancesEnumSort.Consts;
                int instancesIndex = 0;
                foreach (KeyValuePair<string, object> instance in instancesPerClassType)
                {
                    environment.Instances.Add(instance.Key,
                        new InstanceInfo(instancesEnumSortValues[instancesIndex++], objectInstance: instance.Value));
                }
            }

            // Super Types
            foreach (var superType in _superTypes)
            {
                var subTypeSorts = environment.Types.Where(t => t.Key.IsSubclassOf(superType)).Select(t => t.Value).ToList();
                if (subTypeSorts.Count == 0)
                    continue;

                var superTypeConstructors = new List<Constructor>();
                foreach (var subTypeSort in subTypeSorts)
                {
                    var subTypeConstr = context.MkConstructor(
                        name: subTypeSort.Name.ToString(),
                        recognizer: "Is" + subTypeSort.Name,
                        fieldNames: new[] { subTypeSort.Name + "2" + superType.Name },
                        sorts: new[] { subTypeSort },
                        sortRefs: null);
                    superTypeConstructors.Add(subTypeConstr);
                }

                DatatypeSort superTypeSort = context.MkDatatypeSort(superType.Name, superTypeConstructors.ToArray());

                //DatatypeSort = context.MkDatatypeSort("Types", new Constructor[] {
                //    context.MkConstructor("ValTypes", "isValType", new String[] {"Val2Type"}, new Sort[] {ValTypeSort}, null),
                //    context.MkConstructor("RefTypes", "isRefType", new String[] {"Ref2Type"}, new Sort[] {RefTypeSort}, null)
                //});

                environment.Types.Add(superType, superTypeSort);
            }

            // Strings
            if (_possibleStringValues.Any())
            {
                EnumSort enumSort = context.MkEnumSort("Strings", _possibleStringValues.ToArray());
                environment.PossibleStringValues = enumSort;
            }



            foreach (var member in _members)
            {
                LambdaExpressionToConstraintGenerator.GetOrAddMemberAccessFunction(context, environment, member);
            }

            return environment;
        }
    }
}