using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Z3.ObjectTheorem.EF.Metamodel;
using Z3.ObjectTheorem.EF.RoslynIntegration.Assumptions;

namespace Z3.ObjectTheorem.EF.RoslynIntegration
{
    public class EFMetamodelInstantiation
    {
        private const string CollectionFieldPostfix = "CField";
        private const string CollectionOfValueTypePrefix = "CollectionOf";
        private const string ReferenceTypePostfix = "RefType";
        private const string SinglePostfix = "SField";
        private const string ValueFieldPostfix = "VField";
        private const string ValueTypePostfix = "ValueType";

        private readonly AssumptionHandler _assumptionHandler;
        private readonly ConstraintBuilder _constraintBuilder;
        private readonly Dictionary<string, Type> _createdTypes = new Dictionary<string, Type>();
        private readonly Queue<INamedTypeSymbol> _discoveredTypeSymbols = new Queue<INamedTypeSymbol>();
        private readonly IEnumerable<INamedTypeSymbol> _entityTypeSymbols;
        private readonly ObjectTheoremContext _objectTheorem;
        private readonly HashSet<string> _possibleStringValues = new HashSet<string>();
        private readonly PropertyAssumptions _propertyAssumptions;

        public EFMetamodelInstantiation(IEnumerable<INamedTypeSymbol> entityTypeSymbols, ObjectTheoremContext objectTheorem,
            ClassAssumptions classAssumptions, PropertyAssumptions propertyAssumptions)
        {
            _entityTypeSymbols = entityTypeSymbols;
            _objectTheorem = objectTheorem;
            _constraintBuilder = _objectTheorem.ConstraintBuilder;
            _propertyAssumptions = propertyAssumptions;

            _assumptionHandler = new AssumptionHandler(_constraintBuilder, classAssumptions, propertyAssumptions);
        }

        public IEnumerable<KeyValuePair<string, Type>> CreatedTypes { get { return _createdTypes; } }

        public void GenerateConstraints()
        {
            foreach (var entityTypeSymbol in _entityTypeSymbols)
            {
                GenerateConstraintsForType(entityTypeSymbol, isEntity: true);
            }

            while (_discoveredTypeSymbols.Count > 0)
            {
                var discoveredTypeSymbol = _discoveredTypeSymbols.Dequeue();
                if (_entityTypeSymbols.Contains(discoveredTypeSymbol))
                {
                    continue;
                }

                GenerateConstraintsForType(discoveredTypeSymbol, isEntity: false);
            }

            _objectTheorem.SetPossibleStringValues(_possibleStringValues.ToArray());

            foreach (dynamic propertyAssumption in _propertyAssumptions)
            {
                var memberInfo = GetMemberInfo(propertyAssumption.PropertyExpression);
                _objectTheorem.RegisterMember(memberInfo);
            }
        }

        private static MemberInfo GetMemberInfo<T, U>(Expression<System.Func<T, U>> expression)
        {
            var member = expression.Body as MemberExpression;
            if (member != null)
                return member.Member;

            throw new System.ArgumentException("Expression is not a member access", "expression");
        }

        private void GenerateConstraintsForProperty(
            INamedTypeSymbol typeSymbol, ReferenceType referenceType, IPropertySymbol propertySymbol,
            List<ValueField> valueFields, List<SingleField> singleFields, List<CollectionField> collectionFields)
        {
            Field field;

            var name = propertySymbol.Name;
            _possibleStringValues.Add(name);

            var typeName = propertySymbol.Type.Name;
            var propertyAttributes = propertySymbol.GetAttributes();

            if (propertySymbol.Type.IsValueType || "String" == typeName)
            {
                ValueType valType = GetOrAddValueType(typeName);

                var valueField = _objectTheorem.CreateInstance<ValueField>($"{typeSymbol.Name}{name}{ValueFieldPostfix}");
                field = valueField;
                valueFields.Add(valueField);

                _constraintBuilder
                    .Assert(() => valueField.Type == valType);

                _assumptionHandler.HandleValueField(valueField, propertySymbol, propertyAttributes);
            }
            else if (typeName == "ICollection"
                    && (((INamedTypeSymbol)propertySymbol.Type).TypeArguments[0].IsValueType || "String" == ((INamedTypeSymbol)propertySymbol.Type).TypeArguments[0].Name))
            {
                var itemType = ((INamedTypeSymbol)propertySymbol.Type).TypeArguments[0];
                var itemTypeName = CollectionOfValueTypePrefix + itemType.Name;
                ValueType valType = GetOrAddValueType(itemTypeName);

                var valueField = _objectTheorem.CreateInstance<ValueField>($"{typeSymbol.Name}{name}{ValueFieldPostfix}");
                field = valueField;
                valueFields.Add(valueField);

                _constraintBuilder
                    .Assert(() => valueField.Type == valType);
            }
            else
            {
                Type type;
                ReferenceField refField;

                if (typeName == "ICollection")
                {
                    var itemType = ((INamedTypeSymbol)propertySymbol.Type).TypeArguments[0];
                    type = GetOrEnqueueReferencedType((INamedTypeSymbol)itemType, itemType.Name);

                    var collField = _objectTheorem.CreateInstance<CollectionField>($"{typeSymbol.Name}{name}{CollectionFieldPostfix}");
                    collectionFields.Add(collField);
                    refField = collField;
                }
                else
                {
                    type = GetOrEnqueueReferencedType((INamedTypeSymbol)propertySymbol.Type, typeName);

                    var singleRefField = _objectTheorem.CreateInstance<SingleField>($"{typeSymbol.Name}{name}{SinglePostfix}");
                    singleFields.Add(singleRefField);
                    refField = singleRefField;

                    // Need string support of Z3
                    // AddHasForeignKeyAttributeAssumption(classProperty, propertyAttributes, singleRefField);
                }
                var refType = (ReferenceType)type;

                _constraintBuilder
                    .Assert(() => refField.Type == refType);

                field = refField;
            }

            _constraintBuilder
                    .Assert(() => field.Name == name)
                    .Assert(() => field.Owner == referenceType);

            _assumptionHandler.HandleField(field, propertySymbol, propertyAttributes);
        }

        private void GenerateConstraintsForType(INamedTypeSymbol typeSymbol, bool isEntity)
        {
            ReferenceType referenceType = GetOrAddReferenceType(typeSymbol);

            _assumptionHandler.HandleReferenceType(referenceType, typeSymbol, typeSymbol.GetAttributes());

            _constraintBuilder
                    .Assert(() => referenceType.IsEntity == isEntity);

            var valueFields = new List<ValueField>();
            var singleFields = new List<SingleField>();
            var collectionFields = new List<CollectionField>();

            foreach (IPropertySymbol property in typeSymbol.GetMembers().Where(m => m.Kind == SymbolKind.Property))
            {
                GenerateConstraintsForProperty(typeSymbol, referenceType, property, valueFields, singleFields, collectionFields);
            }

            if (collectionFields.Any())
            {
                CollectionField[] classCollectionFields = collectionFields.ToArray();
                _constraintBuilder
                    .Assert(() => referenceType.HasCollectionFields == true)
                    .Assert(() => referenceType.CollectionFields == classCollectionFields);
            }

            if (singleFields.Any())
            {
                SingleField[] classSingleFields = singleFields.ToArray();
                _constraintBuilder
                    .Assert(() => referenceType.HasSingleFields == true)
                    .Assert(() => referenceType.SingleFields == classSingleFields);
            }

            ValueField[] classValueFields = valueFields.ToArray();
            _constraintBuilder
                .Assert(() => referenceType.ValueFields == classValueFields);
        }

        private ReferenceType GetOrAddReferenceType(INamedTypeSymbol entityTypeSymbol)
        {
            var key = entityTypeSymbol.Name + ReferenceTypePostfix;
            Type type;
            if (!_createdTypes.TryGetValue(key, out type))
            {
                _createdTypes[key] = type = _objectTheorem.CreateInstance<ReferenceType>(key);
            }

            return (ReferenceType)type;
        }

        private ValueType GetOrAddValueType(string typeName)
        {
            var key = typeName + ValueTypePostfix;
            Type type;
            if (!_createdTypes.TryGetValue(key, out type))
            {
                _createdTypes[key] = type = _objectTheorem.CreateInstance<ValueType>(key);
            }
            return (Metamodel.ValueType)type;
        }

        private Type GetOrEnqueueReferencedType(INamedTypeSymbol typeSymbol, string typeName)
        {
            var key = typeName + ReferenceTypePostfix;
            Type type;
            if (!_createdTypes.TryGetValue(key, out type))
            {
                _createdTypes[key] = type = _objectTheorem.CreateInstance<ReferenceType>(key);
                _discoveredTypeSymbols.Enqueue(typeSymbol);
            }

            return type;
        }
    }
}