using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CSharpSpeedRush.Models;

namespace CSharpSpeedRush
{
    /// <summary>
    /// Main window for the racing game with enhanced timer functionality
    /// </summary>
    public partial class MainWindow : Window
    {
        private RaceManager _raceManager = null!;
        private DispatcherTimer _gameTimer = null!;
        private TimeSpan _totalRaceTime = TimeSpan.FromMinutes(10);

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        /// <summary>
        /// Initialize the game components and timer
        /// </summary>
        private void InitializeGame()
        {
            _raceManager = new RaceManager();
            _raceManager.StateChanged += RaceManager_StateChanged;

            InitializeTimer();

            var cars = _raceManager.GetAvailableCars();
            CarComboBox.ItemsSource = cars;
            CarComboBox.DisplayMemberPath = "Name";
            CarComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Initialize the race timer for real-time countdown
        /// </summary>
        private void InitializeTimer()
        {
            _gameTimer = new DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromSeconds(1);
            _gameTimer.Tick += GameTimer_Tick;
        }

        /// <summary>
        /// Timer tick event for countdown and fuel decay simulation
        /// </summary>
        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (_raceManager.CurrentState == RaceState.Racing)
            {
                // Decrease time
                _raceManager.RemainingTime = _raceManager.RemainingTime.Subtract(TimeSpan.FromSeconds(1));

                // Update UI on main thread
                Dispatcher.Invoke(() =>
                {
                    UpdateTimeDisplay();

                    // Check if time is up
                    if (_raceManager.RemainingTime <= TimeSpan.Zero)
                    {
                        _gameTimer.Stop();
                        _raceManager.CurrentState = RaceState.OutOfTime;
                        MessageBox.Show("⏰ TIME'S UP! ⏰\n\nThe race is over! You ran out of time.\n\n🔄 Click 'Start Race' to try again!",
                                       "Time Over - Game Over", MessageBoxButton.OK, MessageBoxImage.Warning);
                        ResetGame();
                    }
                });
            }
        }

        private void CarComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CarComboBox.SelectedItem is Car selectedCar)
            {
                _raceManager.SelectCar(CarComboBox.SelectedIndex);
                CarStatsText.Text = $"🏎️ Type: {selectedCar.Type}\n" +
                                   $"🚀 Max Speed: {selectedCar.MaxSpeed} mph\n" +
                                   $"⛽ Fuel Capacity: {selectedCar.FuelCapacity}\n" +
                                   $"📊 Consumption: {selectedCar.FuelConsumptionRate}/action";
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _raceManager.StartRace();
                _gameTimer.Start(); // Start the countdown timer
                StartButton.Content = "🏁 Racing...";
                StartButton.IsEnabled = false;
                CarComboBox.IsEnabled = false;
                EnableActionButtons(true);
                UpdateUI();

                MessageBox.Show("🏁 RACE STARTED! 🏁\n\nGood luck! Manage your fuel wisely!\n\n⛽ Use pit stops when fuel is low!",
                               "Race Begin!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error starting race: {ex.Message}", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SpeedUpButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction(ActionType.SpeedUp);
        }

        private void MaintainButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction(ActionType.Maintain);
        }

        private void PitStopButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteAction(ActionType.PitStop);
        }

        /// <summary>
        /// Execute a race action with comprehensive error handling
        /// </summary>
        private void ExecuteAction(ActionType action)
        {
            try
            {
                _raceManager.ExecuteAction(action);
                UpdateUI();

                // Show action feedback
                string actionMsg = action switch
                {
                    ActionType.SpeedUp => "🚀 Speeding up! Higher fuel consumption.",
                    ActionType.Maintain => "⚡ Maintaining steady pace.",
                    ActionType.PitStop => _raceManager.CurrentState == RaceState.PitStop ?
                                         "🔧 Entering pit stop..." : "⛽ Refueled! Back to racing!",
                    _ => ""
                };

                StatusText.Text = $"{GetStatusEmoji()} {_raceManager.CurrentState} - {actionMsg}";
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Insufficient fuel"))
            {
                MessageBox.Show("⛽ OUT OF FUEL! ⛽\n\nYour car has run out of fuel and cannot continue!\n\n" +
                               "🔧 Strategy tip: Use pit stops before fuel runs completely out!\n\n" +
                               "🔄 Click 'OK' to restart the race and try again!",
                               "Fuel Empty - Game Over", MessageBoxButton.OK, MessageBoxImage.Warning);

                _raceManager.CurrentState = RaceState.OutOfFuel;
                ResetGame();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Action failed: {ex.Message}\n\n💡 Try a different action or use a pit stop!", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RaceManager_StateChanged(object? sender, EventArgs e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Update all UI elements with enhanced visual feedback
        /// </summary>
        private void UpdateUI()
        {
            var car = _raceManager.SelectedCar;
            if (car == null) return;

            // Update status with emoji indicators
            StatusText.Text = $"{GetStatusEmoji()} {_raceManager.CurrentState}";

            // Update lap with completion notifications
            LapText.Text = $"{_raceManager.CurrentLap} / {_raceManager.TotalLaps}";

            // Update fuel with color coding
            double fuelPercent = (double)car.CurrentFuel / car.FuelCapacity * 100;
            FuelProgressBar.Value = fuelPercent;
            FuelText.Text = $"{car.CurrentFuel:F0}/{car.FuelCapacity} ({fuelPercent:F0}%)";

            // Change fuel bar color based on level
            if (fuelPercent <= 20)
                FuelProgressBar.Foreground = System.Windows.Media.Brushes.Red;
            else if (fuelPercent <= 50)
                FuelProgressBar.Foreground = System.Windows.Media.Brushes.Orange;
            else
                FuelProgressBar.Foreground = System.Windows.Media.Brushes.Green;

            // Update race progress
            double racePercent = _raceManager.GetRaceProgress() * 100;
            RaceProgressBar.Value = racePercent;
            ProgressText.Text = $"{racePercent:F1}%";

            // Update time remaining
            UpdateTimeDisplay();

            // Update speed
            SpeedText.Text = $"{car.CurrentSpeed} mph";

            // Update visual progress indicator
            UpdateProgressIndicator();

            // Handle completion of laps (show message when completing each lap)
            if (_raceManager.CurrentLap > 1 && _raceManager.LapProgress == 0.0)
            {
                MessageBox.Show($"🏁 LAP {_raceManager.CurrentLap - 1} COMPLETED! 🏁\n\n" +
                               $"Great job! {_raceManager.TotalLaps - (_raceManager.CurrentLap - 1)} laps remaining!\n\n" +
                               $"⛽ Fuel: {car.CurrentFuel}/{car.FuelCapacity}\n" +
                               $"⏱️ Time: {_raceManager.RemainingTime.Minutes:D2}:{_raceManager.RemainingTime.Seconds:D2}",
                               $"Lap {_raceManager.CurrentLap - 1} Complete!",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Handle race completion (5 laps finished)
            if (_raceManager.CurrentState == RaceState.Finished)
            {
                _gameTimer.Stop();
                var elapsedTime = _totalRaceTime.Subtract(_raceManager.RemainingTime);
                MessageBox.Show($"🏆🎉 CONGRATULATIONS! YOU WON! 🎉🏆\n\n" +
                               $"✅ ALL {_raceManager.TotalLaps} LAPS COMPLETED!\n\n" +
                               $"🏁 Final Statistics:\n" +
                               $"⏱️ Race Time: {elapsedTime.Minutes:D2}:{elapsedTime.Seconds:D2}\n" +
                               $"⛽ Fuel Remaining: {car.CurrentFuel:F0}/{car.FuelCapacity}\n" +
                               $"🏎️ Car Used: {car.Name}\n" +
                               $"🚀 Final Speed: {car.CurrentSpeed} mph\n\n" +
                               $"🌟 VICTORY ACHIEVED! 🌟\n\n" +
                               $"🔄 Click 'OK' to race again!",
                               "🏆 RACE WON! 🏆", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetGame();
            }
            // Handle out of fuel
            else if (_raceManager.CurrentState == RaceState.OutOfFuel)
            {
                _gameTimer.Stop();
                MessageBox.Show("⛽ RACE OVER - OUT OF FUEL! ⛽\n\n" +
                               "Your car ran out of fuel and cannot continue!\n\n" +
                               "💡 Racing Tips for next time:\n" +
                               "• Use pit stops when fuel gets low (below 20%)\n" +
                               "• Balance speed with fuel consumption\n" +
                               "• 'Maintain Speed' uses less fuel than 'Speed Up'\n\n" +
                               "🔄 Ready to try again?",
                               "Game Over - Out of Fuel", MessageBoxButton.OK, MessageBoxImage.Warning);
                ResetGame();
            }
            // Handle out of time
            else if (_raceManager.CurrentState == RaceState.OutOfTime)
            {
                _gameTimer.Stop();
                MessageBox.Show("⏰ RACE OVER - TIME'S UP! ⏰\n\n" +
                               "You ran out of time before completing all laps!\n\n" +
                               "💡 Racing Tips for next time:\n" +
                               "• Use 'Speed Up' more often to cover distance faster\n" +
                               "• Plan your pit stops efficiently\n" +
                               "• Don't spend too much time in pit stops\n\n" +
                               "🔄 Ready for another attempt?",
                               "Game Over - Time Up", MessageBoxButton.OK, MessageBoxImage.Warning);
                ResetGame();
            }
        }

        /// <summary>
        /// Reset the game for a new race
        /// </summary>
        private void ResetGame()
        {
            _gameTimer.Stop();
            StartButton.Content = "🚀 Start Race";
            StartButton.IsEnabled = true;
            CarComboBox.IsEnabled = true;
            EnableActionButtons(false);

            // Reset race manager
            _raceManager = new RaceManager();
            _raceManager.StateChanged += RaceManager_StateChanged;

            // Reselect the car
            if (CarComboBox.SelectedIndex >= 0)
            {
                _raceManager.SelectCar(CarComboBox.SelectedIndex);
            }

            UpdateUI();
        }

        /// <summary>
        /// Get emoji for current race state
        /// </summary>
        private string GetStatusEmoji()
        {
            return _raceManager.CurrentState switch
            {
                RaceState.Racing => "🏁",
                RaceState.PitStop => "🔧",
                RaceState.Finished => "🏆",
                RaceState.OutOfFuel => "⛽",
                RaceState.OutOfTime => "⏰",
                _ => "🎮"
            };
        }

        /// <summary>
        /// Update time remaining display with color coding
        /// </summary>
        private void UpdateTimeDisplay()
        {
            double timePercent = (_raceManager.RemainingTime.TotalSeconds / _totalRaceTime.TotalSeconds) * 100;
            TimeProgressBar.Value = Math.Max(0, timePercent);
            TimeText.Text = $"{_raceManager.RemainingTime.Minutes:D2}:{_raceManager.RemainingTime.Seconds:D2}";

            // Change color based on remaining time
            if (timePercent < 20)
            {
                TimeText.Foreground = System.Windows.Media.Brushes.Red;
                TimeProgressBar.Foreground = System.Windows.Media.Brushes.Red;
            }
            else if (timePercent < 50)
            {
                TimeText.Foreground = System.Windows.Media.Brushes.Orange;
                TimeProgressBar.Foreground = System.Windows.Media.Brushes.Orange;
            }
            else
            {
                TimeText.Foreground = System.Windows.Media.Brushes.Green;
                TimeProgressBar.Foreground = System.Windows.Media.Brushes.Green;
            }
        }

        /// <summary>
        /// Update visual progress indicator
        /// </summary>
        private void UpdateProgressIndicator()
        {
            double progress = _raceManager.GetRaceProgress();
            int position = (int)(progress * 14); // 0 to 14 positions

            var indicator = new string[15]; // Changed from char[] to string[]
            for (int i = 0; i < 15; i++)
            {
                if (i == position && position < 15)
                    indicator[i] = "🏎"; // Changed to string literal
                else if (i < position)
                    indicator[i] = "="; // Changed to string literal
                else if (i == 14)
                    indicator[i] = "🏁"; // Changed to string literal
                else
                    indicator[i] = "-"; // Changed to string literal
            }

            ProgressIndicator.Text = $"[{string.Join("", indicator)}]"; // Join strings instead of chars
        }

        /// <summary>
        /// Enable or disable action buttons
        /// </summary>
        private void EnableActionButtons(bool enabled)
        {
            SpeedUpButton.IsEnabled = enabled;
            MaintainButton.IsEnabled = enabled;
            PitStopButton.IsEnabled = enabled;
        }
    }
}