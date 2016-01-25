using Microsoft.VisualStudio.TestTools.UnitTesting;
using Z3.ObjectTheorem.UnitTests.ComplexModel;
using Z3.ObjectTheorem;
using System.Linq;

namespace Z3.ObjectTheorem.UnitTests
{
    [TestClass]
    public partial class ComplexModelTests
    {
        [TestMethod]
        public void EFMetaModel_WithRefPropsAndAssert_ShouldBe_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var typeInstance1 = objectTheorem.CreateInstance<Category>("TypeInstance1");
            var typeInstance2 = objectTheorem.CreateInstance<Category>("TypeInstance2");
            var fieldInstance1 = objectTheorem.CreateInstance<Product>("FieldInstance1");

            // Act
            objectTheorem.ConstraintBuilder
                .Assert(() => typeInstance1.IsHighlighted == false)
                .Assert(() => fieldInstance1.Category == typeInstance2)
                .Assert(() => fieldInstance1.Category.IsHighlighted == true);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            bool IsEntity1 = solved.GetValue(typeInstance1, i => i.IsHighlighted);
            bool IsEntity2 = solved.GetValue(typeInstance2, i => i.IsHighlighted);
            Assert.IsFalse(IsEntity1);
            Assert.IsTrue(IsEntity2);
        }

        [TestMethod]
        public void EFMetaModel_WithRefPropsAndAssertAll_ShouldBe_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var typeInstance1 = objectTheorem.CreateInstance<Category>("TypeInstance1");
            var fieldInstance1 = objectTheorem.CreateInstance<Product>("FieldInstance1");

            // Act
            objectTheorem.ConstraintBuilder
                .AssertAll<Product>((f) => f.Category.IsHighlighted == true)
                .Assert(() => fieldInstance1.Category == typeInstance1);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            bool IsEntity1 = solved.GetValue(typeInstance1, i => i.IsHighlighted);
            Assert.IsTrue(IsEntity1);
        }

        [TestMethod]
        public void EFMetaModel_WithRefPropsAndAssertAll_ShouldBe_Unsatisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var typeInstance1 = objectTheorem.CreateInstance<Category>("TypeInstance1");
            var fieldInstance1 = objectTheorem.CreateInstance<Product>("FieldInstance1");

            // Act
            objectTheorem.ConstraintBuilder
                .AssertAll<Product>((f) => f.Category.IsHighlighted == true)
                .Assert(() => fieldInstance1.Category == typeInstance1)
                .Assert(() => typeInstance1.IsHighlighted == false);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNull(solved);
        }

        [TestMethod]
        public void EFMetaModel_ShouldBe_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var typeInstance1 = objectTheorem.CreateInstance<Category>("TypeInstance1");
            var typeInstance2 = objectTheorem.CreateInstance<Category>("TypeInstance2");

            var fieldInstance1 = objectTheorem.CreateInstance<Product>("FieldInstance1");
            var fieldInstance2 = objectTheorem.CreateInstance<Product>("FieldInstance2");

            // Act
            objectTheorem.ConstraintBuilder
                    .AssertAll<Product>((f) => !f.Category.IsHighlighted || f.IsPremiumOnly == true)
                    .Assert(() => typeInstance1.IsHighlighted == true)
                    .Assert(() => fieldInstance1.Category == typeInstance1)
                    .Assert(() => fieldInstance2.Category == typeInstance2)
                    .Assert(() => !fieldInstance2.IsPremiumOnly);


            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            bool oneHasKeyAttribute = solved.GetValue(fieldInstance1, i => i.IsPremiumOnly);
            bool twoHasKeyAttribute = solved.GetValue(fieldInstance2, i => i.IsPremiumOnly);
            Assert.IsTrue(oneHasKeyAttribute);
            Assert.IsFalse(twoHasKeyAttribute);
        }
    }
}