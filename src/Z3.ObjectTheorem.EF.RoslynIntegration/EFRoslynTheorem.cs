using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Z3.ObjectTheorem.EF.Metamodel;
using Z3.ObjectTheorem.EF.RoslynIntegration.Assumptions;
using Z3.ObjectTheorem.Solving;

namespace Z3.ObjectTheorem.EF.RoslynIntegration
{
    public class EFRoslynTheorem
    {
        private readonly ClassAssumptions _classAssumptions;
        private readonly ObjectTheoremContext _objectTheorem;
        private readonly PropertyAssumptions _propertyAssumptions;

        public EFRoslynTheorem()
        {
            _classAssumptions = new ClassAssumptions();
            _propertyAssumptions = new PropertyAssumptions();
            _objectTheorem = new ObjectTheoremContext();
        }

        public IEnumerable<IClassAssumption> ClassAssumptions => _classAssumptions.AsReadOnly();

        public IEnumerable<IPropertyAssumption> PropertyAssumptions => _propertyAssumptions.AsReadOnly();

        public void RemoveAssumption(LambdaExpression assumption)
        {
            _objectTheorem.ConstraintBuilder.RemoveAssumption(assumption);
        }

        public ObjectTheoremResult ReSolve()
        {
            return _objectTheorem.Solve();
        }

        public ObjectTheoremResult Solve(IEnumerable<INamedTypeSymbol> entityTypeSymbols)
        {
            _objectTheorem.RegisterSuperType<Metamodel.Type>();
            _objectTheorem.RegisterSuperType<Field>();
            _objectTheorem.RegisterSuperType<ReferenceField>();

            var stopwatch = Stopwatch.StartNew();

            var efMetamodelInstantiation = new EFMetamodelInstantiation(entityTypeSymbols, _objectTheorem, _classAssumptions, _propertyAssumptions);
            efMetamodelInstantiation.GenerateConstraints();

            stopwatch.Stop();
            Trace.WriteLine("Parsing & Metamodel Instantiation: " + stopwatch.Elapsed);

            EFConstraints.AddAll(_objectTheorem, efMetamodelInstantiation.CreatedTypes);

            return _objectTheorem.Solve();
        }
    }
}