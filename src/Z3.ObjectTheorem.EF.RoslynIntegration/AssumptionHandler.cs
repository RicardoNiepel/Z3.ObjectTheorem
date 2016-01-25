using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Z3.ObjectTheorem.EF.Metamodel;
using Z3.ObjectTheorem.EF.RoslynIntegration.Assumptions;

namespace Z3.ObjectTheorem.EF.RoslynIntegration
{
    internal class AssumptionHandler
    {
        private readonly ClassAssumptions _classAssumptions;
        private readonly ConstraintBuilder _constraintBuilder;
        private readonly PropertyAssumptions _propertyAssumptions;

        public AssumptionHandler(ConstraintBuilder constraintBuilder, ClassAssumptions classAssumptions, PropertyAssumptions propertyAssumptions)
        {
            _constraintBuilder = constraintBuilder;

            _classAssumptions = classAssumptions;
            _propertyAssumptions = propertyAssumptions;
        }

        internal void HandleField(Field field, IPropertySymbol property, ImmutableArray<AttributeData> propertyAttributes)
        {
            AddNotMappedAttribute(property, propertyAttributes, field);
        }

        internal void HandleReferenceType(ReferenceType referenceType, INamedTypeSymbol type, ImmutableArray<AttributeData> typeAttributes)
        {
            AddComplexTypeAttribute(type, typeAttributes, referenceType);
        }

        internal void HandleValueField(ValueField valueField, IPropertySymbol property, ImmutableArray<AttributeData> propertyAttributes)
        {
            AddKeyAttribute(property, propertyAttributes, valueField);
            AddIndexAttribute(property, propertyAttributes, valueField);
            AddMaxLengthAttribute(property, propertyAttributes, valueField);
            AddDatabaseGeneratedAttribute(property, propertyAttributes, valueField);
        }

        private void AddComplexTypeAttribute(INamedTypeSymbol classType, IEnumerable<AttributeData> propertyAttributes, ReferenceType refType)
        {
            var hasComplexTypeAttribute = propertyAttributes.Any(a => a.AttributeClass.Name == "ComplexTypeAttribute");
            var assumption = _constraintBuilder.Assume(() => refType.HasComplexTypeAttribute == hasComplexTypeAttribute);
            _classAssumptions.Add(new ComplexTypeAttributeClassAssumption(classType, assumption, refType));
        }

        private void AddDatabaseGeneratedAttribute(IPropertySymbol classProperty, IEnumerable<AttributeData> propertyAttributes, ValueField valueField)
        {
            var databaseGeneratedAttributeValue = DatabaseGeneratedAttribute.None;

            var databaseGeneratedAttribute = propertyAttributes.SingleOrDefault(a => a.AttributeClass.Name == "DatabaseGeneratedAttribute");
            if (databaseGeneratedAttribute != null)
            {
                var databaseGeneratedOption = Enum.GetName(typeof(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption), databaseGeneratedAttribute.ConstructorArguments[0].Value);
                databaseGeneratedAttributeValue = (DatabaseGeneratedAttribute)Enum.Parse(typeof(DatabaseGeneratedAttribute), databaseGeneratedOption);
            }

            var assumption = _constraintBuilder.Assume(() => valueField.DatabaseGeneratedAttribute == databaseGeneratedAttributeValue);
            _propertyAssumptions.Add(new DatabaseGeneratedAttributePropertyAssumption(classProperty, assumption, valueField));
        }

        private void AddHasForeignKeyAttribute(IPropertySymbol classProperty, IEnumerable<AttributeData> propertyAttributes, SingleField singleField)
        {
            var foreignKeyAttribute = propertyAttributes.SingleOrDefault(a => a.AttributeClass.Name == "HasForeignKeyAttribute");

            var foreignKeyAttributeString = "-";
            if (foreignKeyAttribute != null)
            {
                foreignKeyAttributeString = (string)foreignKeyAttribute.ConstructorArguments[0].Value;
            }

            var assumption = _constraintBuilder.Assume(() => singleField.ForeignKeyAttribute == foreignKeyAttributeString);
            _propertyAssumptions.Add(new HasForeignKeyAttributePropertyAssumption(classProperty, assumption, singleField));
        }

        private void AddIndexAttribute(IPropertySymbol classProperty, IEnumerable<AttributeData> propertyAttributes, ValueField valueField)
        {
            var hasIndexAttribute = propertyAttributes.Any(a => a.AttributeClass.Name == "IndexAttribute");
            var assumption = _constraintBuilder.Assume(() => valueField.HasIndexAttribute == hasIndexAttribute);
            _propertyAssumptions.Add(new HasIndexAttributePropertyAssumption(classProperty, assumption, valueField));
        }

        private void AddKeyAttribute(IPropertySymbol classProperty, IEnumerable<AttributeData> propertyAttributes, ValueField valueField)
        {
            var hasKeyAttribute = propertyAttributes.Any(a => a.AttributeClass.Name == "KeyAttribute");
            var assumption = _constraintBuilder.Assume(() => valueField.HasKeyAttribute == hasKeyAttribute);
            _propertyAssumptions.Add(new HasKeyAttributePropertyAssumption(classProperty, assumption, valueField));
        }

        private void AddMaxLengthAttribute(IPropertySymbol classProperty, IEnumerable<AttributeData> propertyAttributes, ValueField valueField)
        {
            var hasMaxLengthAttribute = propertyAttributes.Any(a => a.AttributeClass.Name == "MaxLengthAttribute");
            var assumption = _constraintBuilder.Assume(() => valueField.HasMaxLengthAttribute == hasMaxLengthAttribute);
            _propertyAssumptions.Add(new HasMaxLengthAttributePropertyAssumption(classProperty, assumption, valueField));
        }

        private void AddNotMappedAttribute(IPropertySymbol classProperty, IEnumerable<AttributeData> propertyAttributes, Field field)
        {
            var hasNotMappedAttribute = propertyAttributes.Any(a => a.AttributeClass.Name == "NotMappedAttribute");
            var assumption = _constraintBuilder.Assume(() => field.HasNotMappedAttribute == hasNotMappedAttribute);
            _propertyAssumptions.Add(new HasNotMappedAttributePropertyAssumption(classProperty, assumption, field));
        }
    }
}