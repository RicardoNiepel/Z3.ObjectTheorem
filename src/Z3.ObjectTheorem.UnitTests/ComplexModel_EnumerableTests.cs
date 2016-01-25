using Microsoft.VisualStudio.TestTools.UnitTesting;
using Z3.ObjectTheorem.UnitTests.ComplexModel;
using Z3.ObjectTheorem;
using System.Linq;

namespace Z3.ObjectTheorem.UnitTests
{
    public partial class ComplexModelTests
    {
        [TestMethod]
        [Ignore]
        // Currently not possible > limitation
        public void EFMetaModel_WithAllAndSingleRequiredEmptyArray_ShouldBe_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var typeInstance1 = objectTheorem.CreateInstance<Category>("TypeInstance1");

            var fieldInstance1 = objectTheorem.CreateInstance<Product>("FieldInstance1");

            // Act
            objectTheorem.ConstraintBuilder
                    .AssertAll<Category>(t => t.Products == new [] { fieldInstance1 })
                    .Assert(() => typeInstance1.Products == new [] { fieldInstance1 });

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            System.Collections.Generic.IEnumerable<Product> products1 = solved.GetValue(typeInstance1, i => i.Products);
            Assert.AreEqual(0, products1.Count());
        }

        [TestMethod]
        public void EFMetaModel_WithAllRequiredEmptyArray_ShouldBe_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var typeInstance1 = objectTheorem.CreateInstance<Category>("TypeInstance1");

            var fieldInstance1 = objectTheorem.CreateInstance<Product>("FieldInstance1");

            // Act
            objectTheorem.ConstraintBuilder
                    .AssertAll<Category>(t => t.Products == new Product[0]);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            System.Collections.Generic.IEnumerable<Product> products1 = solved.GetValue(typeInstance1, i => i.Products);
            Assert.AreEqual(0, products1.Count());
        }

        [TestMethod]
        public void EFMetaModel_WithRequiredEmptyArray_ShouldBe_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var typeInstance1 = objectTheorem.CreateInstance<Category>("TypeInstance1");

            var fieldInstance1 = objectTheorem.CreateInstance<Product>("FieldInstance1");

            // Act
            objectTheorem.ConstraintBuilder
                    .Assert(() => typeInstance1.Products == new Product[0]);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            System.Collections.Generic.IEnumerable<Product> products1 = solved.GetValue(typeInstance1, i => i.Products);
            Assert.AreEqual(0, products1.Count());
        }

        [TestMethod]
        public void EFMetaModel_WithAllOneRequiredItemInList_ShouldBe_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var typeInstance1 = objectTheorem.CreateInstance<Category>("TypeInstance1");

            var fieldInstance1 = objectTheorem.CreateInstance<Product>("FieldInstance1");

            // Act
            objectTheorem.ConstraintBuilder
                    .AssertAll<Category>(t => t.Products == new []{ fieldInstance1 });

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            System.Collections.Generic.IEnumerable<Product> products1 = solved.GetValue(typeInstance1, i => i.Products);
            CollectionAssert.AreEquivalent(new[] { fieldInstance1 }, products1.ToList());
        }

        [TestMethod]
        public void EFMetaModel_WithOneRequiredItemInList_ShouldBe_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var typeInstance1 = objectTheorem.CreateInstance<Category>("TypeInstance1");

            var fieldInstance1 = objectTheorem.CreateInstance<Product>("FieldInstance1");

            // Act
            objectTheorem.ConstraintBuilder
                    .Assert(() => typeInstance1.Products == new[] { fieldInstance1 });

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            System.Collections.Generic.IEnumerable<Product> products1 = solved.GetValue(typeInstance1, i => i.Products);
            CollectionAssert.AreEquivalent(new[] { fieldInstance1 }, products1.ToList());
        }

        [TestMethod]
        public void EFMetaModel_WithTwoRequiredItemsInList_ShouldBe_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var typeInstance1 = objectTheorem.CreateInstance<Category>("TypeInstance1");

            var fieldInstance1 = objectTheorem.CreateInstance<Product>("FieldInstance1");
            var fieldInstance2 = objectTheorem.CreateInstance<Product>("FieldInstance2");

            // Act
            objectTheorem.ConstraintBuilder
                    .AssertAll<Category>(t => t.Products == new[] { fieldInstance1, fieldInstance2 });

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            System.Collections.Generic.IEnumerable<Product> products1 = solved.GetValue(typeInstance1, i => i.Products);
            CollectionAssert.AreEquivalent(new[] { fieldInstance1, fieldInstance2 }, products1.ToList());
        }
    }
}