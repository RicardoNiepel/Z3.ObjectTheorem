using System.Collections.Generic;
using System.Linq;
using Z3.ObjectTheorem.EF.Metamodel;

namespace Z3.ObjectTheorem.EF
{
    public static class EFConstraints
    {
        public static void AddAll(ObjectTheoremContext _objectTheorem, IEnumerable<KeyValuePair<string, Metamodel.Type>> createdTypes)
        {
            var stringType = (ValueType)createdTypes.Single(i => i.Key == "StringValueType").Value;
            var numberType = (ValueType)createdTypes.Single(i => i.Key == "Int32ValueType").Value;
            var guidType = (ValueType)createdTypes.Single(i => i.Key == "GuidValueType").Value;
            var collectionsOfValueType = createdTypes
                .Where(t => t.Value is ValueType && t.Key.StartsWith("Collection"))
                .Select(t => t.Value)
                .Cast<ValueType>();

            _objectTheorem.SetPossibleStringValues(new[] { "ID", "Id", "-" });

            var constraintBuilder = _objectTheorem.ConstraintBuilder;

            AddPrimayKeyRuleConstraints(constraintBuilder);
            AddDatabaseGeneratedRuleConstraints(constraintBuilder, stringType, numberType, guidType);
            AddComplexTypeRuleConstraints(constraintBuilder);
            AddIndexRuleConstraints(constraintBuilder, stringType);
            AddNotMappedRuleConstraints(constraintBuilder, collectionsOfValueType);

            //constraintBuilder
            //    .AssertAll<SingleField>(sf => sf.Owner.ValueFields.Any(vf => vf.Name == sf.ForeignKeyAttribute && vf.ForeignType == sf.Type))
            //    .AssertAll<ValueField>(vf => vf.ForeignType.ValueFields.Any(vf2 => vf2.IsPrimaryKey && vf2.Type == vf.Type));
        }

        internal static void AddComplexTypeRuleConstraints(ConstraintBuilder constraintBuilder)
        {
            constraintBuilder
                .AssertAll<ReferenceType>(t => t.IsEntity ^ t.HasComplexTypeAttribute)
                .AssertAll<ReferenceType>(t => !t.HasComplexTypeAttribute ^ t.ValueFields.All(vf => !vf.IsPrimaryKey))
                .AssertAll<ReferenceType>(t => !t.HasComplexTypeAttribute ^ (!t.HasCollectionFields && !t.HasSingleFields));
        }

        internal static void AddDatabaseGeneratedRuleConstraints(
            ConstraintBuilder constraintBuilder, ValueType stringType, ValueType numberType, ValueType guidType)
        {
            constraintBuilder
                .AssertAll<ValueField>(f => (!f.IsPrimaryKey && f.DatabaseGeneratedAttribute == DatabaseGeneratedAttribute.None)
                                            ^
                                            (
                                                f.IsPrimaryKey
                                                &&
                                                (
                                                    (f.Type == stringType && f.DatabaseGeneratedAttribute == DatabaseGeneratedAttribute.Computed)
                                                    ^ (f.Type == numberType && f.DatabaseGeneratedAttribute != DatabaseGeneratedAttribute.Computed)
                                                    ^ (f.Type == guidType && f.DatabaseGeneratedAttribute != DatabaseGeneratedAttribute.Identity)
                                                )
                                            )
                                      );
        }

        internal static void AddIndexRuleConstraints(ConstraintBuilder constraintBuilder, ValueType stringType)
        {
            constraintBuilder
                .AssertAll<ValueField>(vf => (!vf.HasIndexAttribute && !vf.HasMaxLengthAttribute)
                                             ^ (vf.HasIndexAttribute && vf.Type != stringType)
                                             ^ (vf.HasIndexAttribute && vf.HasMaxLengthAttribute))
                .AssertAll<ValueField>(vf => !(vf.HasKeyAttribute && vf.HasIndexAttribute));
        }

        internal static void AddNotMappedRuleConstraints(ConstraintBuilder constraintBuilder, IEnumerable<Metamodel.ValueType> collectionsOfValueType)
        {
            foreach (var collectionOfValueType in collectionsOfValueType)
            {
                constraintBuilder
                    .AssertAll<ValueField>(vf => vf.Type != collectionOfValueType || vf.HasNotMappedAttribute);
            }
            constraintBuilder
                .AssertAll<CollectionField>(cf => !cf.Type.HasComplexTypeAttribute || cf.HasNotMappedAttribute);
        }

        internal static void AddPrimayKeyRuleConstraints(ConstraintBuilder constraintBuilder)
        {
            constraintBuilder
                .AssertAll<ReferenceType>(t => !t.IsEntity
                                                // Primary Key Convention: IdKeyDiscoveryConvention
                                                ^ t.ValueFields.Any(f => (f.Name == "Id" || f.Name == "ID") // TODO: Einschränkung - ohne: || f.Name == "EmployeeTypeId" || f.Name == "EmployeeTypeID"
                                                    && f.IsPrimaryKey
                                                    )
                                                // Primary Key Convention: KeyAttributeConvention
                                                ^ (t.ValueFields.All(fn => fn.Name != "Id" && fn.Name != "ID")
                                                    &&
                                                    t.ValueFields.Any(f1 =>
                                                     f1.HasKeyAttribute
                                                     && f1.IsPrimaryKey
                                                     && t.ValueFields.All(f2 => f2 == f1 || !f2.HasKeyAttribute))
                                                    ))
                // Each entity needs exactly one primary key field
                .AssertAll<ReferenceType>(t => !t.IsEntity
                                                || t.ValueFields.Any(f1 =>
                                                    f1.IsPrimaryKey && t.ValueFields.All(f2 =>
                                                                        f2 == f1 || !f2.IsPrimaryKey)));
        }
    }
}