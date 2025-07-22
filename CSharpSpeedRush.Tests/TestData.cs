using CSharpSpeedRush.Models;
using System;

namespace CSharpSpeedRush.Tests
{
    /// <summary>
    /// Test data factory methods and constants for consistent testing
    /// </summary>
    public static class TestData
    {
        #region Car Factory Methods

        /// <summary>
        /// Creates a standard test car with predictable values
        /// </summary>
        public static Car CreateTestCar()
        {
            return new Car("Test Car", CarType.Economy, 100, 10.0, 50);
        }

        /// <summary>
        /// Creates an economy car with actual game values
        /// </summary>
        public static Car CreateEconomyCar()
        {
            return new Car("Eco Cruiser", CarType.Economy, 80, 8.0, 60);
        }

        /// <summary>
        /// Creates a sport car with actual game values
        /// </summary>
        public static Car CreateSportCar()
        {
            return new Car("Sport Thunder", CarType.Sport, 120, 12.0, 50);
        }

        /// <summary>
        /// Creates a formula car with actual game values
        /// </summary>
        public static Car CreateFormulaCar()
        {
            return new Car("Formula Lightning", CarType.Formula, 160, 18.0, 40);
        }

        /// <summary>
        /// Creates a car with low fuel for testing fuel depletion scenarios
        /// </summary>
        public static Car CreateLowFuelCar()
        {
            var car = CreateTestCar();
            car.CurrentFuel = 5;
            return car;
        }

        /// <summary>
        /// Creates a car with high speed for testing maximum speed scenarios
        /// </summary>
        public static Car CreateHighSpeedCar()
        {
            var car = CreateTestCar();
            car.CurrentSpeed = 95; // Close to max speed of 100
            return car;
        }

        /// <summary>
        /// Creates a car optimized for testing (low consumption, high capacity)
        /// </summary>
        public static Car CreateTestOptimizedCar()
        {
            return new Car("Test Optimized Car", CarType.Economy, 100, 2.0, 100);
        }

        #endregion

        #region RaceManager Factory Methods

        /// <summary>
        /// Creates a race manager with a pre-selected car
        /// </summary>
        public static RaceManager CreateRaceManagerWithSelectedCar()
        {
            var raceManager = new RaceManager();
            var testCar = CreateTestCar();
            raceManager.SelectCar(testCar);
            return raceManager;
        }

        /// <summary>
        /// Creates a race manager in a started state
        /// </summary>
        public static RaceManager CreateStartedRaceManager()
        {
            var raceManager = CreateRaceManagerWithSelectedCar();
            raceManager.StartRace(TimeSpan.FromMinutes(10));
            return raceManager;
        }

        /// <summary>
        /// Creates a race manager with an economy car selected and started
        /// </summary>
        public static RaceManager CreateStartedRaceManagerWithEconomyCar()
        {
            var raceManager = new RaceManager();
            var economyCar = CreateEconomyCar();
            raceManager.SelectCar(economyCar);
            raceManager.StartRace(TimeSpan.FromMinutes(10));
            return raceManager;
        }

        #endregion

        #region Test Constants

        /// <summary>
        /// Standard test values for consistent testing
        /// </summary>
        public static class Constants
        {
            public const int TestCarMaxSpeed = 100;
            public const double TestCarFuelRate = 10.0;
            public const int TestCarFuelCapacity = 50;
            public const int TotalLaps = 5;
            public const double SpeedUpMultiplier = 1.5;
            public const double MaintainMultiplier = 1.0;
            public const double PitStopMultiplier = 0.0;

            public static readonly TimeSpan DefaultRaceDuration = TimeSpan.FromMinutes(10);
            public static readonly TimeSpan ShortRaceDuration = TimeSpan.FromMinutes(5);
            public static readonly TimeSpan LongRaceDuration = TimeSpan.FromMinutes(15);
        }

        #endregion

        #region Action Sequences

        /// <summary>
        /// Returns a sequence of actions that typically advances progress without fuel issues
        /// </summary>
        public static ActionType[] GetSafeProgressSequence()
        {
            return new ActionType[]
            {
                ActionType.SpeedUp,
                ActionType.Maintain,
                ActionType.SpeedUp,
                ActionType.Maintain
            };
        }

        /// <summary>
        /// Returns a sequence of actions for fuel management testing
        /// </summary>
        public static ActionType[] GetFuelManagementSequence()
        {
            return new ActionType[]
            {
                ActionType.SpeedUp,
                ActionType.SpeedUp,
                ActionType.PitStop,
                ActionType.PitStop, // Complete pit stop
                ActionType.SpeedUp
            };
        }

        /// <summary>
        /// Returns a sequence optimized for lap completion
        /// </summary>
        public static ActionType[] GetLapCompletionSequence()
        {
            return new ActionType[]
            {
                ActionType.SpeedUp,
                ActionType.SpeedUp,
                ActionType.SpeedUp,
                ActionType.SpeedUp,
                ActionType.SpeedUp
            };
        }

        #endregion

        #region Validation Helpers

        /// <summary>
        /// Validates that a car has expected default values
        /// </summary>
        public static bool IsValidNewCar(Car car)
        {
            return car != null &&
                   !string.IsNullOrEmpty(car.Name) &&
                   car.CurrentFuel == car.FuelCapacity &&
                   car.CurrentSpeed == 0 &&
                   car.MaxSpeed > 0 &&
                   car.FuelConsumptionRate > 0;
        }

        /// <summary>
        /// Validates that a race manager is in a valid initial state
        /// </summary>
        public static bool IsValidNewRaceManager(RaceManager raceManager)
        {
            return raceManager != null &&
                   raceManager.CurrentState == RaceState.NotStarted &&
                   raceManager.CurrentLap == 1 &&
                   raceManager.LapProgress == 0.0 &&
                   raceManager.TotalLaps == 5 &&
                   raceManager.SelectedCar == null;
        }

        #endregion
    }
}