using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Z3.ObjectTheorem.Solving;

namespace Z3.ObjectTheorem.EF.RoslynIntegration.Assumptions
{
    [DebuggerDisplay("ClassAssumption {Class} {Assumption}")]
    public abstract class ClassAssumption<TObject, TReturn> : IClassAssumption
        where TObject : class
    {
        private ObjectTheoremResult _objectTheoremResult;

        private TReturn _result;

        public ClassAssumption(
            INamedTypeSymbol @class, Expression<Func<bool>> assumption,
            TObject objectToInspect)
        {
            Class = @class;
            Assumption = assumption;
            ObjectToInspect = objectToInspect;
        }

        public Expression<Func<bool>> Assumption { get; }

        public INamedTypeSymbol Class { get; }
        public TObject ObjectToInspect { get; }
        public abstract Expression<Func<TObject, TReturn>> PropertyExpression { get; }
        public abstract int Rank { get; }

        public IEnumerable<string> GetAttributesToAdd(ObjectTheoremResult objectTheoremResult)
        {
            EnsureResult(objectTheoremResult);
            return GetAttributesToAdd(_result);
        }

        public IEnumerable<string> GetAttributesToDelete(ObjectTheoremResult objectTheoremResult)
        {
            EnsureResult(objectTheoremResult);
            return GetAttributesToDelete(_result);
        }

        public TReturn GetValueFromResult(ObjectTheoremResult objectTheoremResult)
        {
            return objectTheoremResult.GetValue(ObjectToInspect, PropertyExpression);
        }

        protected abstract IEnumerable<string> GetAttributesToAdd(TReturn result);

        protected abstract IEnumerable<string> GetAttributesToDelete(TReturn result);

        private void EnsureResult(ObjectTheoremResult objectTheoremResult)
        {
            if (_objectTheoremResult != objectTheoremResult)
            {
                _objectTheoremResult = objectTheoremResult;
                _result = GetValueFromResult(_objectTheoremResult);
            }
        }
    }
}