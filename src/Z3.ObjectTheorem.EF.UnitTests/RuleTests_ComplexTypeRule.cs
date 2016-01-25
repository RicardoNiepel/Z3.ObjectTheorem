using Microsoft.VisualStudio.TestTools.UnitTesting;
using Z3.ObjectTheorem.EF.Metamodel;

namespace Z3.ObjectTheorem.EF.UnitTests
{
    [TestClass]
    public class ComplexTypeRuleTests
    {
        [TestMethod]
        public void ComplexTypeConvention_Successful_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            objectTheorem.RegisterSuperType<Z3.ObjectTheorem.EF.Metamodel.Field>();
            objectTheorem.RegisterSuperType<Z3.ObjectTheorem.EF.Metamodel.Type>();

            var addressType = objectTheorem.CreateInstance<ReferenceType>("AddressType");
            var nameField = objectTheorem.CreateInstance<ValueField>("NameField");

            // Act
            EFConstraints.AddComplexTypeRuleConstraints(objectTheorem.ConstraintBuilder);

            objectTheorem.ConstraintBuilder
                .Assert(() => !addressType.HasCollectionFields && !addressType.HasSingleFields)
                .Assert(() => addressType.ValueFields == new[] { nameField });

            var assume1 = objectTheorem.ConstraintBuilder.Assume(() => addressType.HasComplexTypeAttribute == true);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);
            Assert.AreEqual(Status.Satisfiable, solved.Status);

            bool HasComplexTypeAttribute = solved.GetValue(addressType, i => i.HasComplexTypeAttribute);
            Assert.IsTrue(HasComplexTypeAttribute);
        }

        [TestMethod]
        public void ComplexTypeConvention_Unsuccessful_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            objectTheorem.RegisterSuperType<Z3.ObjectTheorem.EF.Metamodel.Field>();
            objectTheorem.RegisterSuperType<Z3.ObjectTheorem.EF.Metamodel.Type>();
            var addressType = objectTheorem.CreateInstance<ReferenceType>("AddressType");
            var nameField = objectTheorem.CreateInstance<ValueField>("NameField");

            // Act
            EFConstraints.AddComplexTypeRuleConstraints(objectTheorem.ConstraintBuilder);

            objectTheorem.ConstraintBuilder
                .Assert(() => !addressType.HasCollectionFields && addressType.HasSingleFields)
                .Assert(() => addressType.ValueFields == new[] { nameField });

            var assume1 = objectTheorem.ConstraintBuilder.Assume(() => addressType.HasComplexTypeAttribute == true);

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

            bool HasComplexTypeAttribute = solved.GetValue(addressType, i => i.HasComplexTypeAttribute);
            Assert.IsFalse(HasComplexTypeAttribute);
        }
    }
}