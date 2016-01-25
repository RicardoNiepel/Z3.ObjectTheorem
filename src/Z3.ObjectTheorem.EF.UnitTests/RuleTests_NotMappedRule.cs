using Microsoft.VisualStudio.TestTools.UnitTesting;
using Z3.ObjectTheorem.EF.Metamodel;

namespace Z3.ObjectTheorem.EF.UnitTests
{
    [TestClass]
    public class NotMappedRuleTests
    {
        [TestMethod]
        public void NotMappedRule_Successful_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            objectTheorem.RegisterSuperType<Type>();
            objectTheorem.RegisterSuperType<Field>();
            objectTheorem.RegisterSuperType<ReferenceField>();

            var customerType = objectTheorem.CreateInstance<ReferenceType>("Customer1");
            var addressType = objectTheorem.CreateInstance<ReferenceType>("Address1");

            var arrayOfAddressField = objectTheorem.CreateInstance<CollectionField>("AddressField");
            var namesField = objectTheorem.CreateInstance<ValueField>("NamesField");

            var arrayOfStringType = objectTheorem.CreateInstance<ValueType>("CollectionOfStringType");

            // Act
            EFConstraints.AddNotMappedRuleConstraints(objectTheorem.ConstraintBuilder, new[] { arrayOfStringType });

            objectTheorem.ConstraintBuilder
                .Assert(() => customerType.ValueFields == new[] { namesField })
                .Assert(() => namesField.Owner == customerType)
                .Assert(() => namesField.Type == arrayOfStringType)
                .Assert(() => customerType.CollectionFields == new[] { arrayOfAddressField })
                .Assert(() => arrayOfAddressField.Owner == customerType)
                .Assert(() => arrayOfAddressField.Type == addressType)
                .Assert(() => addressType.HasComplexTypeAttribute == true);

            var assume1 = objectTheorem.ConstraintBuilder.Assume(() => arrayOfAddressField.HasNotMappedAttribute == true);
            var assume2 = objectTheorem.ConstraintBuilder.Assume(() => namesField.HasNotMappedAttribute == true);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);
            Assert.AreEqual(Status.Satisfiable, solved.Status);
        }

        [TestMethod]
        public void NotMappedRule_Unsuccessful_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            objectTheorem.RegisterSuperType<Type>();
            objectTheorem.RegisterSuperType<Field>();
            objectTheorem.RegisterSuperType<ReferenceField>();

            var customerType = objectTheorem.CreateInstance<ReferenceType>("Customer1");
            var addressType = objectTheorem.CreateInstance<ReferenceType>("Address1");

            var arrayOfAddressField = objectTheorem.CreateInstance<CollectionField>("AddressField");
            var namesField = objectTheorem.CreateInstance<ValueField>("NamesField");

            var arrayOfStringType = objectTheorem.CreateInstance<ValueType>("CollectionOfStringType");

            // Act
            EFConstraints.AddNotMappedRuleConstraints(objectTheorem.ConstraintBuilder, new[] { arrayOfStringType });

            objectTheorem.ConstraintBuilder
                .Assert(() => customerType.ValueFields == new[] { namesField })
                .Assert(() => namesField.Owner == customerType)
                .Assert(() => namesField.Type == arrayOfStringType)
                .Assert(() => customerType.CollectionFields == new[] { arrayOfAddressField })
                .Assert(() => arrayOfAddressField.Owner == customerType)
                .Assert(() => arrayOfAddressField.Type == addressType)
                .Assert(() => addressType.HasComplexTypeAttribute == true);

            var assume1 = objectTheorem.ConstraintBuilder.Assume(() => arrayOfAddressField.HasNotMappedAttribute == false);
            var assume2 = objectTheorem.ConstraintBuilder.Assume(() => namesField.HasNotMappedAttribute == false);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);
            Assert.AreEqual(Status.Unsatisfiable, solved.Status);

            var assume1Result = solved.IsAssumptionWrong(assume1);
            Assert.IsTrue(assume1Result);
            objectTheorem.ConstraintBuilder.RemoveAssumption(assume1);

            solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);
            Assert.AreEqual(Status.Unsatisfiable, solved.Status);

            var assume2Result = solved.IsAssumptionWrong(assume2);
            Assert.IsTrue(assume2Result);
            objectTheorem.ConstraintBuilder.RemoveAssumption(assume2);

            solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);
            Assert.AreEqual(Status.Satisfiable, solved.Status);

            bool HasNotMappedAttribute1 = solved.GetValue(arrayOfAddressField, f => f.HasNotMappedAttribute);
            bool HasNotMappedAttribute2 = solved.GetValue(namesField, f => f.HasNotMappedAttribute);
            Assert.IsTrue(HasNotMappedAttribute1);
            Assert.IsTrue(HasNotMappedAttribute2);
        }
    }
}