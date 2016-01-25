using Microsoft.VisualStudio.TestTools.UnitTesting;
using Z3.ObjectTheorem.EF.Metamodel;

namespace Z3.ObjectTheorem.EF.UnitTests
{
    [TestClass]
    public class DatabaseGeneratedRuleTests
    {
        [TestMethod]
        public void DatabaseGeneratedRule_Successful_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            objectTheorem.RegisterSuperType<Type>();
            var employeeType = objectTheorem.CreateInstance<ReferenceType>("EmployeeType");

            objectTheorem.RegisterSuperType<Field>();
            objectTheorem.SetPossibleStringValues("Id", "ID", "EmployeeTypeId", "EmployeeTypeID");
            var idField = objectTheorem.CreateInstance<ValueField>("IdField");

            var stringType = objectTheorem.CreateInstance<ValueType>("StringType");
            var numberType = objectTheorem.CreateInstance<ValueType>("NumberType");
            var guidType = objectTheorem.CreateInstance<ValueType>("GuidType");

            // Act
            EFConstraints.AddPrimayKeyRuleConstraints(objectTheorem.ConstraintBuilder);
            EFConstraints.AddDatabaseGeneratedRuleConstraints(objectTheorem.ConstraintBuilder, stringType, numberType, guidType);

            objectTheorem.ConstraintBuilder
                .Assert(() => employeeType.IsEntity == true)
                .Assert(() => employeeType.ValueFields == new[] { idField })
                .Assert(() => idField.Name == "Id")
                .Assert(() => idField.Owner == employeeType)
                .Assert(() => idField.Type == numberType)
                .Assert(() => idField.DatabaseGeneratedAttribute == DatabaseGeneratedAttribute.None);

            var assume1 = objectTheorem.ConstraintBuilder.Assume(() => idField.DatabaseGeneratedAttribute == DatabaseGeneratedAttribute.None);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);
            Assert.AreEqual(Status.Satisfiable, solved.Status);
        }
    }
}