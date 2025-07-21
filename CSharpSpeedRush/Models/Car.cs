using System;

namespace CSharpSpeedRush.Models
{
    public class Car
    {
        public string Name { get; set; }
        public int MaxSpeed { get; set; }
        public double FuelConsumptionRate { get; set; }
        public int FuelCapacity { get; set; }
        public int CurrentFuel { get; set; }
        public CarType Type { get; set; }
        public int CurrentSpeed { get; set; }

        public Car(string name, CarType type, int maxSpeed, double fuelRate, int capacity)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
            MaxSpeed = maxSpeed;
            FuelConsumptionRate = fuelRate;
            FuelCapacity = capacity;
            CurrentFuel = capacity;
            CurrentSpeed = 0;
        }

        public void Refuel() => CurrentFuel = FuelCapacity;

        public void ConsumeFuel(double amount)
        {
            if (amount < 0) throw new ArgumentException("Negative fuel consumption");
            if (CurrentFuel < amount) throw new InvalidOperationException("Insufficient fuel");
            CurrentFuel = Math.Max(0, CurrentFuel - (int)Math.Ceiling(amount));
        }

        public void Reset()
        {
            CurrentFuel = FuelCapacity;
            CurrentSpeed = 0;
        }
    }
}