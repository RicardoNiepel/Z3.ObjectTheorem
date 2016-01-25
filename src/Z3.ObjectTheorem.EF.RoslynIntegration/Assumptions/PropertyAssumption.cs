using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Z3.ObjectTheorem.Solving;

namespace Z3.ObjectTheorem.EF.RoslynIntegration.Assumptions
{
    [DebuggerDisplay("PropertyAssumption {Property} {Assumption}")]
    public abstract class PropertyAssumption<TObject, TReturn> : IPropertyAssumption
        where TObject : class
    {
        private ObjectTheoremResult _objectTheoremResult;

        private TReturn _result;

        public PropertyAssumption(
                            IPropertySymbol property, Expression<Func<bool>> assumption,
            TObject objectToInspect)
        {
            Property = property;
            Assumption = assumption;
            ObjectToInspect = objectToInspect;
        }

        public Expression<Func<bool>> Assumption { get; }

        public TObject ObjectToInspect { get; }

        public IPropertySymbol Property { get; }

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

        public IEnumerable<AttributeSyntax> GetAttributeSyntaxexToAdd(ObjectTheoremResult objectTheoremResult)
        {
            EnsureResult(objectTheoremResult);
            return GetAttributeSyntaxesToAdd(_result);
        }

        public TReturn GetValueFromResult(ObjectTheoremResult objectTheoremResult)
        {
            return objectTheoremResult.GetValue(ObjectToInspect, PropertyExpression);
        }

        protected virtual IEnumerable<string> GetAttributesToAdd(TReturn result)
        {
            return new string[0];
        }

        protected abstract IEnumerable<string> GetAttributesToDelete(TReturn result);

        protected virtual IEnumerable<AttributeSyntax> GetAttributeSyntaxesToAdd(TReturn result)
        {
            return new AttributeSyntax[0];
        }

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