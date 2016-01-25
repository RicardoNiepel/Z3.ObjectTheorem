using Microsoft.VisualStudio.TestTools.UnitTesting;
using Z3.ObjectTheorem.EF.Metamodel;

namespace Z3.ObjectTheorem.EF.UnitTests
{
    [TestClass]
    public class IndexRuleTests
    {
        [TestMethod]
        public void IndexRule_Successful_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            objectTheorem.RegisterSuperType<Type>();
            objectTheorem.RegisterSuperType<Field>();

            var valueField1 = objectTheorem.CreateInstance<ValueField>("ValueField1");

            var stringType = objectTheorem.CreateInstance<ValueType>("StringType");

            // Act
            EFConstraints.AddIndexRuleConstraints(objectTheorem.ConstraintBuilder, stringType);

            objectTheorem.ConstraintBuilder
                .Assert(() => valueField1.Type == stringType);

            var assume1 = objectTheorem.ConstraintBuilder.Assume(() => valueField1.HasIndexAttribute == true);
            var assume2 = objectTheorem.ConstraintBuilder.Assume(() => valueField1.HasMaxLengthAttribute == true);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);
            Assert.AreEqual(Status.Satisfiable, solved.Status);
        }

        [TestMethod]
        public void IndexRule_Unsuccessful_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            objectTheorem.RegisterSuperType<Type>();
            objectTheorem.RegisterSuperType<Field>();

            var valueField1 = objectTheorem.CreateInstance<ValueField>("ValueField1");

            var stringType = objectTheorem.CreateInstance<ValueType>("StringType");

            // Act
            EFConstraints.AddIndexRuleConstraints(objectTheorem.ConstraintBuilder, stringType);

            objectTheorem.ConstraintBuilder
                .Assert(() => valueField1.Type == stringType);

            var assume1 = objectTheorem.ConstraintBuilder.Assume(() => valueField1.HasIndexAttribute == true);
            var assume2 = objectTheorem.ConstraintBuilder.Assume(() => valueField1.HasMaxLengthAttribute == false);

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

            bool HasComplexTypeAttribute = solved.GetValue(valueField1, i => i.HasIndexAttribute);
            Assert.IsFalse(HasComplexTypeAttribute);
        }
    }
}