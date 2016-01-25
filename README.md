# Z3.ObjectTheorem
An ObjectTheorem solver based on Z3 and [LINQ to Z3](https://github.com/RicardoNiepel/Z3.LinqBinding)

## Background
Z3.ObjectTheorem is the ouput of the thesis [Constrained-based diagnostics and code fixes for C\# using Z3, LINQ and Roslyn](https://github.com/RicardoNiepel/Z3.ObjectTheorem/raw/master/docs/Constrained-based%20diagnostics%20and%20code%20fixes%20for%20CSharp%20-%20RicardoNiepel.pdf).

The main topic is the demonstration and evaluation of constrained-based deep fixes with the help of Roslyn diagnostics and code fixes at an Entity Framework 6 code first model. Deep fixes are presented to the user inside Visual Studio 2015. Based on the ideas behind LINQ, C\# Expressions are used as a constraint generation language for object theorems. The Microsoft Z3 Theorem Prover is used for constraint solving.

## Documentation
For a detail documenation, take a look at the thesis [Constrained-based diagnostics and code fixes for C\# using Z3, LINQ and Roslyn](https://github.com/RicardoNiepel/Z3.ObjectTheorem/raw/master/docs/Constrained-based%20diagnostics%20and%20code%20fixes%20for%20CSharp%20-%20RicardoNiepel.pdf).

### Basic Usage
```C#
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
```

**Generated Z3 Input**
```
(declare-datatypes () ((ClassA_instances ClassAInstance)))
(declare-fun IsValidA (ClassA_instances) Bool)

(declare-datatypes () ((ClassB_instances ClassBInstance)))
(declare-fun IsValidB (ClassB_instances) Bool)

(assert
  (forall ((ClassA ClassA_instances)(ClassB ClassB_instances))
  (= (IsValidA ClassA) (IsValidB ClassB))
))
(assert (= (IsValidA ClassAInstance) true))
(assert (= (IsValidB ClassBInstance) false))
```

### Usage with Strings
Because Z3 does not support String type, it is necessary to define all possible String values upfront.

```C#
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
```

### Usage of Navigation Properties
Z3.ObjectTheorem treated navigation properties the same way as value type properties.

```C#
// Arrange
var objectTheorem = new ObjectTheoremContext();

var typeInstance1 = objectTheorem.CreateInstance<(*@\tc{Category}@*)>("TypeInstance1");
var typeInstance2 = objectTheorem.CreateInstance<(*@\tc{Category}@*)>("TypeInstance2");
var fieldInstance1 = objectTheorem.CreateInstance<Product>("FieldInstance1");

// Act
objectTheorem.ConstraintBuilder
    .Assert(() => typeInstance1.IsHighlighted == false)
    .Assert(() => fieldInstance1.Category == typeInstance2)
    .Assert(() => fieldInstance1.Category.IsHighlighted == true);

// Assert
var solved = objectTheorem.Solve();
Assert.IsNotNull(solved);

bool IsEntity1 = solved.GetValue(typeInstance1, i => i.IsHighlighted);
bool IsEntity2 = solved.GetValue(typeInstance2, i => i.IsHighlighted);
Assert.IsFalse(IsEntity1);
Assert.IsTrue(IsEntity2);
```

### Usage of Class Hierarchies
To use class hierarchies, it is necessary to register the super types, if no instances are created from them (e.g. if using abstract super types).
Super types can be used the same way as types without class hierarchy can be.

```C#
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
(*@\tc{Assert}@*).IsNotNull(solved);

int bicycle1Speed = solved.GetValue(bicycleInstance1, i => i.Speed);
int bicycle2Speed = solved.GetValue(bicycleInstance2, i => i.Speed);
int carSpeed = solved.GetValue(carInstance1, i => i.Speed);

Assert.AreEqual(10, bicycle1Speed);
Assert.IsTrue(bicycle2Speed > 0);

Assert.IsTrue(carSpeed > 0);
Assert.IsTrue(bicycle1Speed < carSpeed);
Assert.IsTrue(bicycle2Speed < carSpeed);
```

### Usage of Assumptions
In addition to create constraints, which are built by Assert and AssertAll, it is also possible to use assumptions.
If the theorem is unsatisfiable, each assumption can be checked if it is wrong and can be removed from the theorem.
If the solver is able to solve the theorem, the value of the property, which is used inside the assumption, can be retrieved.

```C#
// Arrange
var objectTheorem = new ObjectTheoremContext();

var carInstance1 = objectTheorem.CreateInstance<Car>("CarInstance1");

// Act
objectTheorem.ConstraintBuilder
    .AssertAll<Car>(c => c.IsFast == true);

var assume1 = objectTheorem.ConstraintBuilder
    .Assume(() => carInstance1.IsFast == false);

// Assert
var solved = objectTheorem.Solve();
Assert.AreEqual(Status.Unsatisfiable, solved.Status);

var assume1Result = solved.IsAssumptionWrong(assume1);
Assert.IsTrue(assume1Result);
objectTheorem.ConstraintBuilder.RemoveAssumption(assume1);

solved = objectTheorem.Solve();
Assert.AreEqual(Status.Satisfiable, solved.Status);

bool isFast1 = solved.GetValue(carInstance1, c => c.IsFast);
Assert.IsTrue(isFast1);
```
