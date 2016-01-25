using Microsoft.VisualStudio.TestTools.UnitTesting;
using Z3.ObjectTheorem;
using System.Linq;
using Z3.ObjectTheorem.UnitTests.HierarchyModel;

namespace Z3.ObjectTheorem.UnitTests
{
    [TestClass]
    public partial class HierarchyModelTests
    {
        [TestMethod]
        public void HM_WithRefPropsAndAssert_ShouldBe_Satisfiable()
        {
            // Arrange
            var objectTheorem = new ObjectTheoremContext();

            objectTheorem.RegisterSuperType<Vehicle>();
            var bicycleInstance1 = objectTheorem.CreateInstance<Bicycle>("BicycleInstance1");
            var bicycleInstance2 = objectTheorem.CreateInstance<Bicycle>("BicycleInstance2");
            var carInstance1 = objectTheorem.CreateInstance<Car>("CarInstance1");

            // Act
            objectTheorem.ConstraintBuilder
                .AssertAll<Vehicle>(v => v.Speed > 0)
                .AssertAll<Bicycle, Car>((b, c) => b.Speed < c.Speed)
                .Assert(() => bicycleInstance1.Speed == 10);

            // Assert
            var solved = objectTheorem.Solve();
            Assert.IsNotNull(solved);

            int bicycle1Speed = solved.GetValue(bicycleInstance1, i => i.Speed);
            int bicycle2Speed = solved.GetValue(bicycleInstance2, i => i.Speed);
            int carSpeed = solved.GetValue(carInstance1, i => i.Speed);

            Assert.AreEqual(10, bicycle1Speed);
            Assert.IsTrue(bicycle2Speed > 0);
            
            Assert.IsTrue(carSpeed > 0);
            Assert.IsTrue(bicycle1Speed < carSpeed);
            Assert.IsTrue(bicycle2Speed < carSpeed);
        }
    }
}