using System;
using System.Linq.Expressions;

namespace Z3.ObjectTheorem
{
    public class ConstraintBuilder
    {
        private ObjectTheoremContext _objectTheorem;

        internal ConstraintBuilder(ObjectTheoremContext objectTheorem)
        {
            _objectTheorem = objectTheorem;
        }

        public ConstraintBuilder Assert(Expression<Func<bool>> constraint)
        {
            _objectTheorem.AddAssertion(constraint);
            return this;
        }

        public ConstraintBuilder AssertAll<T1>(Expression<Func<T1, bool>> constraint)
        {
            _objectTheorem.AddAllAssertion(constraint);
            return this;
        }

        public ConstraintBuilder AssertAll<T1, T2>(Expression<Func<T1, T2, bool>> constraint)
        {
            _objectTheorem.AddAllAssertion(constraint);
            return this;
        }

        public Expression<Func<bool>> Assume(Expression<Func<bool>> constraint)
        {
            _objectTheorem.AddAssumption(constraint);
            return constraint;
        }

        public Expression<Func<T1, T2, bool>> AssumeAll<T1, T2>(Expression<Func<T1, T2, bool>> constraint)
        {
            _objectTheorem.AddAllAssumption(constraint);
            return constraint;
        }

        public void RemoveAssumption(LambdaExpression assumption)
        {
            _objectTheorem.RemoveAssumption(assumption);
        }
    }
}