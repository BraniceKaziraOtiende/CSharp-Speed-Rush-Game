using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpSpeedRush.Models
{
    public class RaceManager
    {
        private readonly List<Car> _cars;
        private readonly Queue<ActionType> _actionHistory;

        public Car? SelectedCar { get; private set; }
        public RaceState CurrentState { get; internal set; } = RaceState.NotStarted;
        public int CurrentLap { get; private set; } = 1;
        public double LapProgress { get; private set; } = 0.0;
        public int TotalLaps { get; } = 5;
        public TimeSpan RemainingTime { get; private set; }
        public TimeSpan TotalRaceTime { get; private set; }

        public event EventHandler? StateChanged;

        public RaceManager()
        {
            _cars = new List<Car>
            {
                new Car("Eco Cruiser", CarType.Economy, 80, 8.0, 60),
                new Car("Sport Thunder", CarType.Sport, 120, 12.0, 50),
                new Car("Formula Lightning", CarType.Formula, 160, 18.0, 40)
            };
            _actionHistory = new Queue<ActionType>();
            TotalRaceTime = TimeSpan.FromMinutes(10);
        }

        public List<Car> GetAvailableCars() => _cars.ToList();

        public void SelectCar(Car car)
        {
            if (CurrentState != RaceState.NotStarted)
                throw new InvalidOperationException("Cannot change car during race");

            SelectedCar = car ?? throw new ArgumentNullException(nameof(car));
            SelectedCar.Reset();
        }

        public void StartRace(TimeSpan raceDuration)
        {
            if (SelectedCar == null)
                throw new InvalidOperationException("No car selected");

            TotalRaceTime = raceDuration;
            RemainingTime = raceDuration;
            CurrentState = RaceState.Racing;
            CurrentLap = 1;
            LapProgress = 0.0;
            SelectedCar.Reset();
            _actionHistory.Clear();

            OnStateChanged("Race started!");
        }

        public void ExecuteAction(ActionType action)
        {
            if (CurrentState != RaceState.Racing && CurrentState != RaceState.PitStop)
                throw new InvalidOperationException("Race is not active");

            if (SelectedCar == null)
                throw new InvalidOperationException("No car selected");

            switch (action)
            {
                case ActionType.SpeedUp:
                    HandleSpeedUp();
                    break;
                case ActionType.Maintain:
                    HandleMaintainSpeed();
                    break;
                case ActionType.PitStop:
                    HandlePitStop();
                    break;
            }

            _actionHistory.Enqueue(action);
            UpdateRaceState();
        }

        private void HandleSpeedUp()
        {
            if (SelectedCar == null) return;

            double fuelCost = SelectedCar.FuelConsumptionRate * 1.5;
            SelectedCar.ConsumeFuel(fuelCost);
            SelectedCar.CurrentSpeed = Math.Min(SelectedCar.MaxSpeed, SelectedCar.CurrentSpeed + 20);
            LapProgress += 0.25;
            OnStateChanged("Speed increased!");
        }

        private void HandleMaintainSpeed()
        {
            if (SelectedCar == null) return;

            SelectedCar.ConsumeFuel(SelectedCar.FuelConsumptionRate);
            LapProgress += 0.15;
            OnStateChanged("Maintaining speed");
        }

        private void HandlePitStop()
        {
            if (SelectedCar == null) return;

            if (CurrentState == RaceState.PitStop)
            {
                SelectedCar.Refuel();
                CurrentState = RaceState.Racing;
                OnStateChanged("Refuel complete! Back to racing!");
            }
            else
            {
                CurrentState = RaceState.PitStop;
                SelectedCar.CurrentSpeed = 0;
                OnStateChanged("Entering pit stop...");
            }
        }

        public void AdvanceTime(TimeSpan timeElapsed)
        {
            if (CurrentState != RaceState.Racing || SelectedCar == null) return;

            RemainingTime = RemainingTime.Subtract(timeElapsed);

            if (RemainingTime < TimeSpan.Zero)
            {
                RemainingTime = TimeSpan.Zero;
            }

            double passiveFuelUse = SelectedCar.CurrentSpeed * 0.005;
            SelectedCar.CurrentSpeed = (int)Math.Round(Math.Max(0, SelectedCar.CurrentSpeed - 0.5));
            SelectedCar.ConsumeFuel(passiveFuelUse);
            LapProgress += 0.02;

            UpdateRaceState();
        }

        private void UpdateRaceState()
        {
            if (LapProgress >= 1.0)
            {
                CurrentLap++;
                LapProgress = 0.0;
                OnStateChanged($"Lap {CurrentLap} started!");
            }

            if (CurrentLap > TotalLaps)
            {
                CurrentState = RaceState.Finished;
                OnStateChanged("Race completed!");
            }
            else if (SelectedCar?.CurrentFuel <= 0)
            {
                CurrentState = RaceState.OutOfFuel;
                OnStateChanged("Out of fuel!");
            }
            else if (RemainingTime <= TimeSpan.Zero)
            {
                CurrentState = RaceState.OutOfTime;
                OnStateChanged("Time's up!");
            }
        }

        public double GetRaceProgress() => ((CurrentLap - 1) + LapProgress) / TotalLaps;

        public TimeSpan GetElapsedTime() => TotalRaceTime - RemainingTime;

        protected virtual void OnStateChanged(string message)
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}