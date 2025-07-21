namespace CSharpSpeedRush.Models
{
    /// <summary>
    /// Types of cars available
    /// </summary>
    public enum CarType { Economy, Sport, Formula }

    /// <summary>
    /// Current race state
    /// </summary>
    public enum RaceState { NotStarted, Racing, PitStop, Finished, OutOfFuel, OutOfTime }

    /// <summary>
    /// Player actions
    /// </summary>
    public enum ActionType { SpeedUp, Maintain, PitStop }
}
