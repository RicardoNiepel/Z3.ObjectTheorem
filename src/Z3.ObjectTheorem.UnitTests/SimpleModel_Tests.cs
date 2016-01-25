using Microsoft.VisualStudio.TestTools.UnitTesting;
using Z3.ObjectTheorem.UnitTests.SimpleModel;

namespace Z3.ObjectTheorem.UnitTests
{
    // TODO: add tests with integers
    [TestClass]
    public class SimpleModelTests
    {
        [TestMethod]
        public void EnumValues_ShouldBe_Satisfiable1()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var classAInstance1 = objectTheorem.CreateInstance<ClassA>("ClassAInstance");
            var classAInstance2 = objectTheorem.CreateInstance<ClassA>("ClassBInstance");

            // Act
            objectTheorem.ConstraintBuilder
                    .Assert(() => classAInstance1.Color == Colors.Black)
                    .Assert(() => Colors.Green == classAInstance2.Color);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            Colors color1 = solved.GetValue(classAInstance1, i => i.Color);
            Colors color2 = solved.GetValue(classAInstance2, i => i.Color);
            Assert.AreEqual(Colors.Black, color1);
            Assert.AreEqual(Colors.Green, color2);
        }

        [TestMethod]
        public void EnumValues_ShouldBe_Satisfiable2()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var classAInstance1 = objectTheorem.CreateInstance<ClassA>("ClassAInstance");
            var classAInstance2 = objectTheorem.CreateInstance<ClassA>("ClassBInstance");

            // Act
            objectTheorem.ConstraintBuilder
                    .Assert(() => classAInstance1.Color != classAInstance2.Color)
                    .Assert(() => classAInstance1.Color == Colors.Black);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            Colors color1 = solved.GetValue(classAInstance1, i => i.Color);
            Colors color2 = solved.GetValue(classAInstance2, i => i.Color);
            Assert.IsTrue(color1 != color2);
            Assert.AreEqual(Colors.Black, color1);
        }

        [TestMethod]
        public void EnumValuesFromOutside_ShouldBe_Satisfiable2()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var classAInstance1 = objectTheorem.CreateInstance<ClassA>("ClassAInstance");
            var classAInstance2 = objectTheorem.CreateInstance<ClassA>("ClassBInstance");

            // Act
            var color = Colors.Black;
            objectTheorem.ConstraintBuilder
                    .Assert(() => classAInstance1.Color != classAInstance2.Color)
                    .Assert(() => classAInstance1.Color == color);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            Colors color1 = solved.GetValue(classAInstance1, i => i.Color);
            Colors color2 = solved.GetValue(classAInstance2, i => i.Color);
            Assert.IsTrue(color1 != color2);
            Assert.AreEqual(color, color1);
        }

        [TestMethod]
        public void SimpleModel_OutsideConstant_ShouldBe_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var outsideBoolConstant = true;

            var classAInstance = objectTheorem.CreateInstance<ClassA>("ClassAInstance");
            var classBInstance = objectTheorem.CreateInstance<ClassB>("ClassBInstance");

            // Act
            objectTheorem.ConstraintBuilder
                    .AssertAll<ClassA, ClassB>((a, b) => a.IsValidA == b.IsValidB)
                    .Assert(() => classAInstance.IsValidA == outsideBoolConstant);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            bool isValidA = solved.GetValue(classAInstance, i => i.IsValidA);
            bool isValidB = solved.GetValue(classBInstance, i => i.IsValidB);
            Assert.IsTrue(isValidA);
            Assert.IsTrue(isValidB);
        }

        [TestMethod]
        public void SimpleModel_ShouldBe_NotSatisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var classAInstance = objectTheorem.CreateInstance<ClassA>("ClassAInstance");
            var classBInstance = objectTheorem.CreateInstance<ClassB>("ClassBInstance");

            // Act
            objectTheorem.ConstraintBuilder
                    .AssertAll<ClassA, ClassB>((a, b) => a.IsValidA == b.IsValidB)
                    .Assert(() => classAInstance.IsValidA == true)
                    .Assert(() => classBInstance.IsValidB == false);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNull(solved);
        }

        [TestMethod]
        public void SimpleModel_ShouldBe_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var classAInstance = objectTheorem.CreateInstance<ClassA>("ClassAInstance");
            var classBInstance = objectTheorem.CreateInstance<ClassB>("ClassBInstance");

            // Act
            objectTheorem.ConstraintBuilder
                    .AssertAll<ClassA, ClassB>((a, b) => a.IsValidA == b.IsValidB)
                    .Assert(() => classAInstance.IsValidA == true);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            bool isValidA = solved.GetValue(classAInstance, i => i.IsValidA);
            bool isValidB = solved.GetValue(classBInstance, i => i.IsValidB);
            Assert.IsTrue(isValidA);
            Assert.IsTrue(isValidB);
        }
    }
}