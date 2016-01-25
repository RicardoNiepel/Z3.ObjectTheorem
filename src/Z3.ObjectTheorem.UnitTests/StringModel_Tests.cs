using Microsoft.VisualStudio.TestTools.UnitTesting;
using Z3.ObjectTheorem.UnitTests.StringModel;

namespace Z3.ObjectTheorem.UnitTests
{
    [TestClass]
    public partial class StringModelTests
    {
        [TestMethod]
        public void SM_WithStringProps_ShouldBe_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            objectTheorem.SetPossibleStringValues("Hans", "Fred", "Max");

            var classWithStringA = objectTheorem.CreateInstance<ClassWithStringA>("ClassWithStringA");

            // Act
            objectTheorem.ConstraintBuilder
                .AssertAll<ClassWithStringA>(c => c.FirstName != "Hans")
                .Assert(() => "Fred" != classWithStringA.FirstName);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            string firstName = solved.GetValue(classWithStringA, i => i.FirstName);
            Assert.AreEqual("Max", firstName);
        }

        [TestMethod]
        public void SM_WithStringProps_ShouldBe_Unsatisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            objectTheorem.SetPossibleStringValues("Hans", "Fred");

            var classWithStringA = objectTheorem.CreateInstance<ClassWithStringA>("ClassWithStringA");

            // Act
            objectTheorem.ConstraintBuilder
                .AssertAll<ClassWithStringA>(c => c.FirstName != "Hans")
                .Assert(() => "Fred" != classWithStringA.FirstName);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNull(solved);
        }

        [TestMethod]
        public void SM_WithStringPropsAndSamePossibleStrings_ShouldBe_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var stringNames = new[] { "Hans", "Fred", "Max" };

            objectTheorem.SetPossibleStringValues(stringNames);

            var classWithStringA = objectTheorem.CreateInstance<ClassWithStringA>("ClassWithStringA");
            var classWithStringB = objectTheorem.CreateInstance<ClassWithStringB>("ClassWithStringB");

            // Act
            objectTheorem.ConstraintBuilder
                .AssertAll<ClassWithStringA, ClassWithStringB>((a, b) => a.FirstName == b.LastName)
                .Assert(() => "Hans" == classWithStringA.FirstName);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            string firstName = solved.GetValue(classWithStringA, i => i.FirstName);
            string lastName = solved.GetValue(classWithStringB, i => i.LastName);
            Assert.AreEqual("Hans", firstName);
            Assert.AreEqual("Hans", lastName);
        }

        [TestMethod]
        public void SM_WithStringPropsAndSamePossibleStrings_ShouldBe_Unsatisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            var stringNames = new[] { "Hans", "Fred", "Max" };

            objectTheorem.SetPossibleStringValues(stringNames);

            var classWithStringA = objectTheorem.CreateInstance<ClassWithStringA>("ClassWithStringA");
            var classWithStringB = objectTheorem.CreateInstance<ClassWithStringB>("ClassWithStringB");

            // Act
            objectTheorem.ConstraintBuilder
                .AssertAll<ClassWithStringA, ClassWithStringB>((a, b) => a.FirstName == b.LastName)
                .Assert(() => "Hans" == classWithStringA.FirstName)
                .Assert(() => classWithStringB.LastName == "Fred");

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNull(solved);
        }
    }
}