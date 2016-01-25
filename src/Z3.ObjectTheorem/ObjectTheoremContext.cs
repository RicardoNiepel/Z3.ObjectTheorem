using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Z3.ObjectTheorem.Solving;

namespace Z3.ObjectTheorem
{
    public class ObjectTheoremContext
    {
        private readonly List<LambdaExpression> _allAssertions = new List<LambdaExpression>();
        private readonly List<LambdaExpression> _allAssumptions = new List<LambdaExpression>();
        private readonly List<LambdaExpression> _assertions = new List<LambdaExpression>();
        private readonly List<LambdaExpression> _assumptions = new List<LambdaExpression>();
        private readonly Dictionary<string, object> _instances = new Dictionary<string, object>();
        private readonly List<MemberInfo> _members = new List<MemberInfo>();
        private readonly HashSet<string> _possibleStringValues = new HashSet<string>();
        private readonly List<Type> _superTypes = new List<Type>();

        public ConstraintBuilder ConstraintBuilder { get { return new ConstraintBuilder(this); } }

        public T CreateInstance<T>(string instanceName) where T : class, new()
        {
            var instance = new T();

            _instances.Add(instanceName, instance);

            return instance;
        }

        public void RegisterMember(MemberInfo memberInfo)
        {
            _members.Add(memberInfo);
        }

        public void RegisterSuperType<T>() where T : class
        {
            _superTypes.Add(typeof(T));
        }

        public void SetPossibleStringValues(params string[] stringValues)
        {
            foreach (var item in stringValues)
            {
                _possibleStringValues.Add(item);
            }
        }

        public ObjectTheoremResult Solve()
        {
            var solver = new ObjectTheoremSolver(
                _instances, _superTypes, _possibleStringValues.ToList(), _members,
                _assertions, _assumptions, _allAssertions, _allAssumptions);

            return solver.Solve();
        }

        internal void AddAllAssertion<T1>(Expression<System.Func<T1, bool>> constraint)
        {
            _allAssertions.Add(constraint);
        }

        internal void AddAllAssertion<T1, T2>(Expression<System.Func<T1, T2, bool>> constraint)
        {
            _allAssertions.Add(constraint);
        }

        internal void AddAllAssumption<T1, T2>(Expression<System.Func<T1, T2, bool>> constraint)
        {
            _allAssumptions.Add(constraint);
        }

        internal void AddAssertion(LambdaExpression constraint)
        {
            _assertions.Add(constraint);
        }

        internal void AddAssumption(LambdaExpression constraint)
        {
            _assumptions.Add(constraint);
        }

        internal void RemoveAssumption(LambdaExpression assumption)
        {
            if (!_assumptions.Remove(assumption))
            {
                if (!_allAssumptions.Remove(assumption))
                {
                    throw new InvalidOperationException("assumption " + assumption + " cannot be removed, because it does not exist");
                }
            }
        }
    }
}