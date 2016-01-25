using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Z3.ObjectTheorem.EF.Metamodel;

namespace Z3.ObjectTheorem.EF.UnitTests
{
    [TestClass]
    public class PrimayKeyRuleTests
    {
        [TestMethod]
        public void IdKeyDiscoveryConvention_And_KeyAttributeConvention_Unsuccessful_Unsatisfiable_()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            objectTheorem.RegisterSuperType<Z3.ObjectTheorem.EF.Metamodel.Type>();
            var employeeType = objectTheorem.CreateInstance<ReferenceType>("EmployeeType");

            objectTheorem.RegisterSuperType<Field>();
            objectTheorem.SetPossibleStringValues("Id", "Title", "Name", "Income", "ID", "EmployeeTypeId", "EmployeeTypeID");
            var titleField = objectTheorem.CreateInstance<ValueField>("TitleField");
            var nameField = objectTheorem.CreateInstance<ValueField>("NameField");
            var incomeField = objectTheorem.CreateInstance<ValueField>("IncomeField");

            // Act
            EFConstraints.AddPrimayKeyRuleConstraints(objectTheorem.ConstraintBuilder);

            objectTheorem.ConstraintBuilder
                .Assert(() => employeeType.IsEntity == true)
                .Assert(() => employeeType.ValueFields == new[] { titleField, nameField, incomeField })
                .Assert(() => titleField.Name == "Title")
                .Assert(() => titleField.Owner == employeeType)
                .Assert(() => nameField.Name == "Name")
                .Assert(() => nameField.Owner == employeeType)
                .Assert(() => incomeField.Name == "Income")
                .Assert(() => incomeField.Owner == employeeType);

            var assume1 = objectTheorem.ConstraintBuilder.Assume(() => titleField.HasKeyAttribute == false);
            var assume2 = objectTheorem.ConstraintBuilder.Assume(() => nameField.HasKeyAttribute == false);
            var assume3 = objectTheorem.ConstraintBuilder.Assume(() => incomeField.HasKeyAttribute == false);

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

            bool HasKeyAttribute1 = solved.GetValue(titleField, i => i.HasKeyAttribute);
            bool HasKeyAttribute2 = solved.GetValue(nameField, i => i.HasKeyAttribute);
            bool HasKeyAttribute3 = solved.GetValue(incomeField, i => i.HasKeyAttribute);
            Assert.IsTrue(HasKeyAttribute1);
            Assert.IsFalse(HasKeyAttribute2);
            Assert.IsFalse(HasKeyAttribute3);

            bool IsPrimaryKey1 = solved.GetValue(titleField, i => i.IsPrimaryKey);
            bool IsPrimaryKey2 = solved.GetValue(nameField, i => i.IsPrimaryKey);
            bool IsPrimaryKey3 = solved.GetValue(incomeField, i => i.IsPrimaryKey);
            Assert.IsTrue(IsPrimaryKey1);
            Assert.IsFalse(IsPrimaryKey2);
            Assert.IsFalse(IsPrimaryKey3);
        }

        [TestMethod]
        public void IdKeyDiscoveryConvention_Successful_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            objectTheorem.RegisterSuperType<Z3.ObjectTheorem.EF.Metamodel.Type>();
            var employeeType = objectTheorem.CreateInstance<ReferenceType>("EmployeeType");

            objectTheorem.RegisterSuperType<Field>();
            objectTheorem.SetPossibleStringValues("Id", "Name", "Income", "ID", "EmployeeTypeId", "EmployeeTypeID");
            var idField = objectTheorem.CreateInstance<ValueField>("IdField");
            var nameField = objectTheorem.CreateInstance<ValueField>("NameField");
            var incomeField = objectTheorem.CreateInstance<ValueField>("IncomeField");

            // Act
            EFConstraints.AddPrimayKeyRuleConstraints(objectTheorem.ConstraintBuilder);

            objectTheorem.ConstraintBuilder
                .Assert(() => employeeType.IsEntity == true)
                .Assert(() => employeeType.ValueFields == new[] { idField, nameField, incomeField })
                .Assert(() => idField.Name == "Id")
                .Assert(() => idField.Owner == employeeType)
                .Assert(() => nameField.Name == "Name")
                .Assert(() => nameField.Owner == employeeType)
                .Assert(() => incomeField.Name == "Income")
                .Assert(() => incomeField.Owner == employeeType);

            var assume1 = objectTheorem.ConstraintBuilder.Assume(() => idField.HasKeyAttribute == false);
            var assume2 = objectTheorem.ConstraintBuilder.Assume(() => nameField.HasKeyAttribute == false);
            var assume3 = objectTheorem.ConstraintBuilder.Assume(() => incomeField.HasKeyAttribute == false);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);
            Assert.AreEqual(Status.Satisfiable, solved.Status);

            bool HasKeyAttribute1 = solved.GetValue(idField, i => i.HasKeyAttribute);
            bool HasKeyAttribute2 = solved.GetValue(nameField, i => i.HasKeyAttribute);
            bool HasKeyAttribute3 = solved.GetValue(incomeField, i => i.HasKeyAttribute);
            Assert.IsFalse(HasKeyAttribute1);
            Assert.IsFalse(HasKeyAttribute2);
            Assert.IsFalse(HasKeyAttribute3);

            bool IsPrimaryKey1 = solved.GetValue(idField, i => i.IsPrimaryKey);
            bool IsPrimaryKey2 = solved.GetValue(nameField, i => i.IsPrimaryKey);
            bool IsPrimaryKey3 = solved.GetValue(incomeField, i => i.IsPrimaryKey);
            Assert.IsTrue(IsPrimaryKey1);
            Assert.IsFalse(IsPrimaryKey2);
            Assert.IsFalse(IsPrimaryKey3);
        }
    }
}