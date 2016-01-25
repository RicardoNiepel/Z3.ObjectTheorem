using Microsoft.VisualStudio.TestTools.UnitTesting;
using Z3.ObjectTheorem.EF.Metamodel;
using System.Linq;

namespace Z3.ObjectTheorem.EF.UnitTests
{
    [TestClass]
    public class ForeignKeyDiscoveryRuleTests
    {
        [TestMethod]
        public void ForeignKeyDiscoveryConvention_Successful_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            objectTheorem.RegisterSuperType<Type>();
            objectTheorem.RegisterSuperType<Field>();
            objectTheorem.RegisterSuperType<ReferenceField>();

            var nullType = objectTheorem.CreateInstance<ReferenceType>("NullType");

            var categoryType = objectTheorem.CreateInstance<ReferenceType>("CategoryType");
            var categoryIdField = objectTheorem.CreateInstance<ValueField>("CategoryIdField");

            var productType = objectTheorem.CreateInstance<ReferenceType>("ProductType");
            var productIdField = objectTheorem.CreateInstance<ValueField>("ProductIdField");

            var productCategoryIdField = objectTheorem.CreateInstance<ValueField>("productCategoryIdField");
            var productCategoryField = objectTheorem.CreateInstance<SingleField>("productCategoryField");

            var stringType = objectTheorem.CreateInstance<ValueType>("StringType");
            var numberType = objectTheorem.CreateInstance<ValueType>("NumberType");
            var guidType = objectTheorem.CreateInstance<ValueType>("GuidType");

            objectTheorem.SetPossibleStringValues("CatId", "ProdId", "CategoryId", "Category", "-");

            // Act
            objectTheorem.ConstraintBuilder
                .AssertAll<SingleField>(sf => sf.Owner.ValueFields.Any(vf => vf.Name == sf.ForeignKeyAttribute && vf.ForeignType == sf.Type))
                .AssertAll<ValueField>(vf => vf.ForeignType == nullType ^ vf.ForeignType.ValueFields.Any(vf2 => vf2.IsPrimaryKey && vf2.Type == vf.Type))

                .Assert(() => categoryIdField.Owner == categoryType)
                .Assert(() => categoryIdField.Name == "CatId")
                .Assert(() => categoryIdField.Type == stringType)
                .Assert(() => categoryIdField.IsPrimaryKey == true)
                .Assert(() => categoryType.ValueFields == new[] { categoryIdField })

                .Assert(() => productIdField.Owner == productType)
                .Assert(() => productIdField.Name == "ProdId")
                .Assert(() => productIdField.Type == guidType)
                .Assert(() => productIdField.IsPrimaryKey == true)
                .Assert(() => productCategoryIdField.Owner == productType)
                .Assert(() => productCategoryIdField.Name == "CategoryId")
                .Assert(() => productCategoryIdField.Type == stringType)
                .Assert(() => productCategoryField.Owner == productType)
                .Assert(() => productCategoryField.Name == "Category")
                .Assert(() => productCategoryField.Type == categoryType)
                .Assert(() => productType.ValueFields == new[] { productIdField, productCategoryIdField })
                .Assert(() => productType.SingleFields == new[] { productCategoryField });

            var assume1 = objectTheorem.ConstraintBuilder.Assume(() => productCategoryField.ForeignKeyAttribute == "CategoryId");

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);
            Assert.AreEqual(Status.Satisfiable, solved.Status);

            var test = solved.GetValue(productCategoryIdField, p => p.Type);
            Assert.AreEqual(stringType, test);
        }
    }
}