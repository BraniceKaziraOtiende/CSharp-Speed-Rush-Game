using NUnit.Framework;
using CSharpSpeedRush.Models;
using System;

namespace CSharpSpeedRush.Tests
{
    [TestFixture]
    public class CarTests
    {
        private Car _testCar = null!;

        [SetUp]
        public void Setup()
        {
            _testCar = new Car("Test Car", CarType.Economy, 100, 10.0, 50);
        }

        [Test]
        public void Car_Constructor_InitializesPropertiesCorrectly()
        {
            Assert.That(_testCar.Name, Is.EqualTo("Test Car"));
            Assert.That(_testCar.Type, Is.EqualTo(CarType.Economy));
            Assert.That(_testCar.MaxSpeed, Is.EqualTo(100));
            Assert.That(_testCar.FuelConsumptionRate, Is.EqualTo(10.0));
            Assert.That(_testCar.FuelCapacity, Is.EqualTo(50));
            Assert.That(_testCar.CurrentFuel, Is.EqualTo(50));
            Assert.That(_testCar.CurrentSpeed, Is.EqualTo(0));
        }

        [Test]
        public void Car_Constructor_NullName_ThrowsException()
        {
            Assert.That(() => new Car(null!, CarType.Economy, 100, 10.0, 50),
                       Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Refuel_SetsCurrentFuelToCapacity()
        {
            _testCar.CurrentFuel = 20;
            _testCar.Refuel();
            Assert.That(_testCar.CurrentFuel, Is.EqualTo(_testCar.FuelCapacity));
        }

        [Test]
        public void ConsumeFuel_ValidAmount_ReducesFuel()
        {
            var initialFuel = _testCar.CurrentFuel;
            var consumeAmount = 15.0;

            _testCar.ConsumeFuel(consumeAmount);

            Assert.That(_testCar.CurrentFuel, Is.EqualTo(initialFuel - consumeAmount).Within(1.0));
        }

        [Test]
        public void ConsumeFuel_NegativeAmount_ThrowsException()
        {
            Assert.That(() => _testCar.ConsumeFuel(-5.0),
                       Throws.TypeOf<ArgumentException>()
                       .With.Message.Contains("negative"));
        }

        [Test]
        public void ConsumeFuel_InsufficientFuel_ThrowsException()
        {
            _testCar.CurrentFuel = 10;
            Assert.That(() => _testCar.ConsumeFuel(15.0),
                       Throws.TypeOf<InvalidOperationException>()
                       .With.Message.Contains("Insufficient fuel"));
        }

        [Test]
        public void ConsumeFuel_ExactAmount_ReducesToZero()
        {
            var fuelAmount = _testCar.CurrentFuel;
            _testCar.ConsumeFuel(fuelAmount);
            Assert.That(_testCar.CurrentFuel, Is.EqualTo(0));
        }

        [Test]
        public void ConsumeFuel_FractionalAmount_RoundsUpCorrectly()
        {
            var initialFuel = _testCar.CurrentFuel;
            var consumeAmount = 10.7; // Should round up to 11

            _testCar.ConsumeFuel(consumeAmount);

            Assert.That(_testCar.CurrentFuel, Is.EqualTo(initialFuel - 11));
        }

        [Test]
        public void ConsumeFuel_ZeroAmount_NoChange()
        {
            var initialFuel = _testCar.CurrentFuel;
            _testCar.ConsumeFuel(0.0);
            Assert.That(_testCar.CurrentFuel, Is.EqualTo(initialFuel));
        }

        [Test]
        public void Car_DifferentTypes_HaveDifferentCharacteristics()
        {
            var economyCar = new Car("Eco", CarType.Economy, 80, 8.0, 60);
            var sportCar = new Car("Sport", CarType.Sport, 120, 12.0, 50);
            var formulaCar = new Car("Formula", CarType.Formula, 160, 18.0, 40);

            // Economy car should have lowest consumption, highest capacity
            Assert.That(economyCar.FuelConsumptionRate, Is.LessThan(sportCar.FuelConsumptionRate));
            Assert.That(economyCar.FuelCapacity, Is.GreaterThan(formulaCar.FuelCapacity));

            // Formula car should have highest speed and consumption
            Assert.That(formulaCar.MaxSpeed, Is.GreaterThan(economyCar.MaxSpeed));
            Assert.That(formulaCar.FuelConsumptionRate, Is.GreaterThan(economyCar.FuelConsumptionRate));
        }
    }
}
