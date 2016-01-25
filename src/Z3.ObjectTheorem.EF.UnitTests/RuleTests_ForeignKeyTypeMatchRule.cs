using Microsoft.VisualStudio.TestTools.UnitTesting;
using Z3.ObjectTheorem.EF.Metamodel;

namespace Z3.ObjectTheorem.EF.UnitTests
{
    [TestClass]
    public class ForeignKeyTypeMatchRuleTests
    {
        [TestMethod]
        public void ForeignKeyTypeMatchRule_Unsuccessful_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            objectTheorem.RegisterSuperType<Type>();
            objectTheorem.RegisterSuperType<Field>();
            objectTheorem.RegisterSuperType<ReferenceField>();

            var productType = objectTheorem.CreateInstance<ReferenceType>("ProductType");
            var categoryType = objectTheorem.CreateInstance<ReferenceType>("CategoryType");

            var stringType = objectTheorem.CreateInstance<ValueType>("StringType");
            var guidType = objectTheorem.CreateInstance<ValueType>("GuidType");

            var categoryField = objectTheorem.CreateInstance<SingleField>("CategoryField");
            var categoryIdField = objectTheorem.CreateInstance<ValueField>("CategoryIdField");
            var idField = objectTheorem.CreateInstance<ValueField>("IdField");

            objectTheorem.SetPossibleStringValues("ID", "Id", "CategoryId");

            // Act
            objectTheorem.ConstraintBuilder
                .Assert(() => categoryIdField.Type == idField.Type)

                .Assert(() => productType.SingleFields == new[] { categoryField })
                .Assert(() => categoryField.Owner == productType)
                .Assert(() => categoryField.Type == categoryType)
                .Assert(() => productType.ValueFields == new[] { categoryIdField })
                .Assert(() => categoryIdField.Owner == productType)
                .Assert(() => categoryIdField.Name == "CategoryId")
                .Assert(() => categoryType.ValueFields == new[] { idField })
                .Assert(() => idField.Owner == categoryType)
                .Assert(() => idField.Name == "Id")
                .Assert(() => idField.Type == guidType);

            var assume1 = objectTheorem.ConstraintBuilder.Assume(() => categoryIdField.Type == stringType);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);
            Assert.AreEqual(Status.Unsatisfiable, solved.Status);

            var assume1Result = solved.IsAssumptionWrong(assume1);
            Assert.IsTrue(assume1Result);
            objectTheorem.ConstraintBuilder.RemoveAssumption(assume1);

            solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);
            Assert.AreEqual(Status.Satisfiable, solved.Status);

            ValueType Type1 = solved.GetValue(categoryIdField, f => f.Type);
            Assert.AreEqual(guidType, Type1);
        }
    }
}