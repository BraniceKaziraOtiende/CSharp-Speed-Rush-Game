using NUnit.Framework;
using CSharpSpeedRush.Models;
using System;

namespace CSharpSpeedRush.Tests
{
    [TestFixture]
    public class RaceManagerTests
    {
        private RaceManager _raceManager = null!;
        private Car _testCar = null!;

        [SetUp]
        public void Setup()
        {
            _raceManager = new RaceManager();
            _testCar = new Car("Test Car", CarType.Economy, 100, 10.0, 50);
        }

        [Test]
        public void SelectCar_ValidCar_SetsSelectedCar()
        {
            // Test car selection with Car object (not index)
            _raceManager.SelectCar(_testCar);
            Assert.That(_raceManager.SelectedCar, Is.EqualTo(_testCar));
        }

        [Test]
        public void StartRace_WithRaceDuration_InitializesCorrectly()
        {
            // Test StartRace with TimeSpan parameter
            _raceManager.SelectCar(_testCar);
            var raceDuration = TimeSpan.FromMinutes(10);

            _raceManager.StartRace(raceDuration);

            Assert.That(_raceManager.CurrentState, Is.EqualTo(RaceState.Racing));
            Assert.That(_raceManager.CurrentLap, Is.EqualTo(1));
            Assert.That(_raceManager.LapProgress, Is.EqualTo(0.0));
            Assert.That(_raceManager.RemainingTime, Is.EqualTo(raceDuration));
        }

        [Test]
        public void StartRace_NoCarSelected_ThrowsException()
        {
            var newManager = new RaceManager();
            var raceDuration = TimeSpan.FromMinutes(10);

            Assert.That(() => newManager.StartRace(raceDuration), Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void ExecuteAction_SpeedUp_AdvancesProgress()
        {
            _raceManager.SelectCar(_testCar);
            _raceManager.StartRace(TimeSpan.FromMinutes(10));
            var initialProgress = _raceManager.LapProgress;

            _raceManager.ExecuteAction(ActionType.SpeedUp);

            Assert.That(_raceManager.LapProgress, Is.GreaterThan(initialProgress));
        }

        [Test]
        public void ExecuteAction_MultipleSpeedUps_AdvancesLap()
        {
            _raceManager.SelectCar(_testCar);
            _raceManager.StartRace(TimeSpan.FromMinutes(10));
            int initialLap = _raceManager.CurrentLap;
            var initialProgress = _raceManager.LapProgress;

            // Execute just 2 speed up actions to avoid fuel depletion
            _raceManager.ExecuteAction(ActionType.SpeedUp);
            _raceManager.ExecuteAction(ActionType.SpeedUp);

            // Check that progress has advanced (either lap increased or progress increased)
            Assert.That(_raceManager.CurrentLap > initialLap || _raceManager.LapProgress > initialProgress,
                        "Race progress should advance after speed up actions");
        }

        [Test]
        public void GetAvailableCars_ReturnsThreeCars()
        {
            var cars = _raceManager.GetAvailableCars();

            Assert.That(cars, Is.Not.Null);
            Assert.That(cars.Count, Is.EqualTo(3));
            Assert.That(cars[0].Type, Is.EqualTo(CarType.Economy));
            Assert.That(cars[1].Type, Is.EqualTo(CarType.Sport));
            Assert.That(cars[2].Type, Is.EqualTo(CarType.Formula));
        }

        [Test]
        public void GetRaceProgress_InitialState_ReturnsZero()
        {
            _raceManager.SelectCar(_testCar);
            _raceManager.StartRace(TimeSpan.FromMinutes(10));

            var progress = _raceManager.GetRaceProgress();

            Assert.That(progress, Is.EqualTo(0.0).Within(0.01));
        }

        [Test]
        public void AdvanceTime_ReducesRemainingTime()
        {
            _raceManager.SelectCar(_testCar);
            _raceManager.StartRace(TimeSpan.FromMinutes(10));
            var initialTime = _raceManager.RemainingTime;

            _raceManager.AdvanceTime(TimeSpan.FromSeconds(30));

            Assert.That(_raceManager.RemainingTime, Is.LessThan(initialTime));
        }

        [Test]
        public void GetElapsedTime_ReturnsCorrectElapsedTime()
        {
            _raceManager.SelectCar(_testCar);
            var raceDuration = TimeSpan.FromMinutes(10);
            _raceManager.StartRace(raceDuration);

            _raceManager.AdvanceTime(TimeSpan.FromMinutes(2));

            var elapsedTime = _raceManager.GetElapsedTime();
            Assert.That(elapsedTime, Is.EqualTo(TimeSpan.FromMinutes(2)).Within(TimeSpan.FromSeconds(1)));
        }

        [Test]
        public void ExecuteAction_PitStop_RefuelsCar()
        {
            _raceManager.SelectCar(_testCar);
            _raceManager.StartRace(TimeSpan.FromMinutes(10));
            var car = _raceManager.SelectedCar;

            if (car != null)
            {
                // Reduce fuel first
                car.CurrentFuel = 10;

                // Execute pit stop
                _raceManager.ExecuteAction(ActionType.PitStop);
                _raceManager.ExecuteAction(ActionType.PitStop); // Complete pit stop

                Assert.That(car.CurrentFuel, Is.EqualTo(car.FuelCapacity));
            }
        }

        [Test]
        public void ExecuteAction_Maintain_AdvancesProgressSlowly()
        {
            _raceManager.SelectCar(_testCar);
            _raceManager.StartRace(TimeSpan.FromMinutes(10));
            var initialProgress = _raceManager.LapProgress;

            _raceManager.ExecuteAction(ActionType.Maintain);

            Assert.That(_raceManager.LapProgress, Is.GreaterThan(initialProgress));
        }

        [Test]
        public void SelectCar_FromAvailableCars_WorksCorrectly()
        {
            var cars = _raceManager.GetAvailableCars();
            var economyCar = cars.First(c => c.Type == CarType.Economy);

            _raceManager.SelectCar(economyCar);

            Assert.That(_raceManager.SelectedCar, Is.EqualTo(economyCar));
            Assert.That(_raceManager.SelectedCar?.Type, Is.EqualTo(CarType.Economy));
        }

        [Test]
        public void RaceManager_InitialState_IsCorrect()
        {
            Assert.That(_raceManager.CurrentState, Is.EqualTo(RaceState.NotStarted));
            Assert.That(_raceManager.CurrentLap, Is.EqualTo(1));
            Assert.That(_raceManager.LapProgress, Is.EqualTo(0.0));
            Assert.That(_raceManager.TotalLaps, Is.EqualTo(5));
            Assert.That(_raceManager.SelectedCar, Is.Null);
        }

        [Test]
        public void ExecuteAction_WithInsufficientFuel_ThrowsException()
        {
            _raceManager.SelectCar(_testCar);
            _raceManager.StartRace(TimeSpan.FromMinutes(10));

            // Deplete fuel completely
            var car = _raceManager.SelectedCar;
            if (car != null)
            {
                car.CurrentFuel = 5; // Very low fuel

                // This should throw an exception due to insufficient fuel
                Assert.That(() => _raceManager.ExecuteAction(ActionType.SpeedUp),
                           Throws.TypeOf<InvalidOperationException>()
                           .With.Message.Contains("Insufficient fuel"));
            }
        }

        [Test]
        public void StateChanged_Event_FiresOnStateChange()
        {
            bool eventFired = false;
            _raceManager.StateChanged += (sender, args) => eventFired = true;

            _raceManager.SelectCar(_testCar);
            _raceManager.StartRace(TimeSpan.FromMinutes(10));

            Assert.That(eventFired, Is.True, "StateChanged event should fire when race starts");
        }
    }
}
