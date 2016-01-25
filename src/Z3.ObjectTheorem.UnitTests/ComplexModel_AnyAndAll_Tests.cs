using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Z3.ObjectTheorem.UnitTests.ComplexModel;

namespace Z3.ObjectTheorem.UnitTests
{
    public partial class ComplexModelTests
    {
        [TestMethod]
        public void WithAnyAndAll_TwoItemArray_ShouldBe_Satisfiable1()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var typeInstance1 = objectTheorem.CreateInstance<Category>("TypeInstance1");

            var fieldInstance1 = objectTheorem.CreateInstance<Product>("FieldInstance1");
            var fieldInstance2 = objectTheorem.CreateInstance<Product>("FieldInstance2");

            // Act
            objectTheorem.ConstraintBuilder
                    .AssertAll<Category>(t => t.Products.Any(f1 => f1.IsPremiumOnly == true
                                          && t.Products.All(f2 => f2 == f1 || f2.IsPremiumOnly == false)))
                    .Assert(() => typeInstance1.Products == new[] { fieldInstance1 })
                    .Assert(() => fieldInstance1.Category == typeInstance1)
                    .Assert(() => fieldInstance2.IsPremiumOnly == false);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            bool oneHasKeyAttribute = solved.GetValue(fieldInstance1, i => i.IsPremiumOnly);
            bool twoHasKeyAttribute = solved.GetValue(fieldInstance2, i => i.IsPremiumOnly);
            Assert.IsTrue(oneHasKeyAttribute);
            Assert.IsFalse(twoHasKeyAttribute);
        }

        [TestMethod]
        public void WithAnyAndAll_TwoItemArray_ShouldBe_Satisfiable2()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var typeInstance1 = objectTheorem.CreateInstance<Category>("TypeInstance1");

            var fieldInstance1 = objectTheorem.CreateInstance<Product>("FieldInstance1");
            var fieldInstance2 = objectTheorem.CreateInstance<Product>("FieldInstance2");

            // Act
            objectTheorem.ConstraintBuilder
                    .AssertAll<Category>(t => t.Products.Any(f1 => f1.IsPremiumOnly == true
                                          && t.Products.All(f2 => f2 == f1 || f2.IsPremiumOnly == false)))
                    .Assert(() => typeInstance1.Products == new[] { fieldInstance1 })
                    .Assert(() => fieldInstance1.Category == typeInstance1)
                    .Assert(() => fieldInstance1.IsPremiumOnly == true)
                    .Assert(() => fieldInstance2.IsPremiumOnly == false);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            bool oneHasKeyAttribute = solved.GetValue(fieldInstance1, i => i.IsPremiumOnly);
            bool twoHasKeyAttribute = solved.GetValue(fieldInstance2, i => i.IsPremiumOnly);
            Assert.IsTrue(oneHasKeyAttribute);
            Assert.IsFalse(twoHasKeyAttribute);
        }

        [TestMethod]
        public void WithAnyAndAll_TwoItemArray_ShouldBe_Satisfiable3()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var typeInstance1 = objectTheorem.CreateInstance<Category>("TypeInstance1");

            var fieldInstance1 = objectTheorem.CreateInstance<Product>("FieldInstance1");
            var fieldInstance2 = objectTheorem.CreateInstance<Product>("FieldInstance2");

            // Act
            objectTheorem.ConstraintBuilder
                    .AssertAll<Category>(t => t.Products.Any(f1 => f1.IsPremiumOnly == true
                                          && t.Products.All(f2 => f2 == f1 || f2.IsPremiumOnly == false)))
                    .Assert(() => typeInstance1.Products == new[] { fieldInstance1, fieldInstance2 })
                    .Assert(() => fieldInstance1.Category == typeInstance1)
                    .Assert(() => fieldInstance2.Category == typeInstance1)
                    .Assert(() => fieldInstance1.IsPremiumOnly == true)
                    .Assert(() => fieldInstance2.IsPremiumOnly == false);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            bool oneHasKeyAttribute = solved.GetValue(fieldInstance1, i => i.IsPremiumOnly);
            bool twoHasKeyAttribute = solved.GetValue(fieldInstance2, i => i.IsPremiumOnly);
            Assert.IsTrue(oneHasKeyAttribute);
            Assert.IsFalse(twoHasKeyAttribute);
        }

        [TestMethod]
        public void WithAnyAndAll_TwoItemArray_ShouldBe_Unsatisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var typeInstance1 = objectTheorem.CreateInstance<Category>("TypeInstance1");

            var fieldInstance1 = objectTheorem.CreateInstance<Product>("FieldInstance1");
            var fieldInstance2 = objectTheorem.CreateInstance<Product>("FieldInstance2");

            // Act
            objectTheorem.ConstraintBuilder
                    .AssertAll<Category>(t => t.Products.Any(f1 => f1.IsPremiumOnly == true
                                          && t.Products.All(f2 => f2 == f1 || f2.IsPremiumOnly == false)))
                    .Assert(() => typeInstance1.Products == new[] { fieldInstance1, fieldInstance2 })
                    .Assert(() => fieldInstance1.Category == typeInstance1)
                    .Assert(() => fieldInstance2.Category == typeInstance1)
                    .Assert(() => fieldInstance1.IsPremiumOnly == true)
                    .Assert(() => fieldInstance2.IsPremiumOnly == true);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNull(solved);
        }

        [TestMethod]
        public void WithAnyAndAll_TwoItemArray_ShouldBe_Unsatisfiable2()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var typeInstance1 = objectTheorem.CreateInstance<Category>("TypeInstance1");

            var fieldInstance1 = objectTheorem.CreateInstance<Product>("FieldInstance1");
            var fieldInstance2 = objectTheorem.CreateInstance<Product>("FieldInstance2");

            // Act
            objectTheorem.ConstraintBuilder
                    .AssertAll<Category>(t => t.Products.Any(f1 => f1.IsPremiumOnly == true
                                          && t.Products.All(f2 => f2 == f1 || f2.IsPremiumOnly == false)))
                    .Assert(() => typeInstance1.Products == new[] { fieldInstance1, fieldInstance2 })
                    .Assert(() => fieldInstance1.Category == typeInstance1)
                    .Assert(() => fieldInstance2.Category == typeInstance1)
                    .Assert(() => fieldInstance1.IsPremiumOnly == false)
                    .Assert(() => fieldInstance2.IsPremiumOnly == false);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNull(solved);
        }
    }
}