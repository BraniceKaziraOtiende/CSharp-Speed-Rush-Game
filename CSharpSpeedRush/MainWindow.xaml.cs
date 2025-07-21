using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CSharpSpeedRush.Models;

namespace CSharpSpeedRush
{
    public partial class MainWindow : Window
    {
        private readonly RaceManager _raceManager;
        private readonly DispatcherTimer _gameTimer;
        private readonly TimeSpan _totalRaceTime = TimeSpan.FromMinutes(10);

        public MainWindow()
        {
            InitializeComponent();
            _raceManager = new RaceManager();
            _gameTimer = new DispatcherTimer();
            InitializeGame();
        }

        private void InitializeGame()
        {
            _raceManager.StateChanged += RaceManager_StateChanged;
            InitializeTimer();
            PopulateCarSelection();
            UpdateUI();
        }

        private void InitializeTimer()
        {
            _gameTimer.Interval = TimeSpan.FromSeconds(1);
            _gameTimer.Tick += GameTimer_Tick;
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (_raceManager.CurrentState == RaceState.Racing)
            {
                _raceManager.AdvanceTime(TimeSpan.FromSeconds(1));
                UpdateUI();

                if (_raceManager.RemainingTime <= TimeSpan.Zero)
                {
                    EndRace("⏰ Time's Up!\nYou ran out of time!\nFinal Lap: " + _raceManager.CurrentLap);
                }
                else if (_raceManager.SelectedCar?.CurrentFuel <= 0)
                {
                    EndRace("⛽ Out of Fuel!\nRace Over!\nFinal Lap: " + _raceManager.CurrentLap);
                }
                else if (_raceManager.CurrentLap > _raceManager.TotalLaps)
                {
                    var timeTaken = _raceManager.TotalRaceTime - _raceManager.RemainingTime;
                    EndRace($"🏆 Race Completed!\nTime: {timeTaken:mm\\:ss}\nCongratulations!");
                }
            }
        }

        private void PopulateCarSelection()
        {
            CarComboBox.ItemsSource = _raceManager.GetAvailableCars();
            CarComboBox.DisplayMemberPath = "Name";
            CarComboBox.SelectedIndex = 0;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CarComboBox.SelectedItem is Car selectedCar)
                {
                    _raceManager.SelectCar(selectedCar);
                    _raceManager.StartRace(_totalRaceTime);
                    _gameTimer.Start();
                    StartButton.IsEnabled = false;
                    CarComboBox.IsEnabled = false;
                    EnableActionButtons(true);
                    AddToLog("🏁 Race started! Good luck!");
                    RaceCompleteOverlay.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting race: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EndRace(string message)
        {
            _gameTimer.Stop();
            EnableActionButtons(false);
            RaceCompleteOverlay.Visibility = Visibility.Visible;
            RaceResultText.Text = message;
            UpdateUI();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            RaceCompleteOverlay.Visibility = Visibility.Collapsed;
            StartButton.IsEnabled = true;
            StartButton.Content = "🚀 Start Race";
            CarComboBox.IsEnabled = true;
        }

        private void ExecuteAction(ActionType action)
        {
            try
            {
                _raceManager.ExecuteAction(action);
                UpdateUI();
                AddToLog($"Action: {action} executed");
            }
            catch (Exception ex)
            {
                AddToLog($"⚠️ Error: {ex.Message}");
            }
        }

        private void UpdateUI()
        {
            if (_raceManager.SelectedCar == null) return;

            // Update time display
            TimeText.Text = _raceManager.RemainingTime.ToString(@"mm\:ss");
            TimeProgressBar.Value = _raceManager.RemainingTime.TotalSeconds;

            // Update other UI elements
            StatusText.Text = _raceManager.CurrentState.ToString();
            LapText.Text = $"{_raceManager.CurrentLap}/{_raceManager.TotalLaps}";
            SpeedText.Text = $"{_raceManager.SelectedCar.CurrentSpeed} mph";
            FuelText.Text = $"{_raceManager.SelectedCar.CurrentFuel}/{_raceManager.SelectedCar.FuelCapacity}";

            FuelProgressBar.Value = _raceManager.SelectedCar.CurrentFuel;
            FuelProgressBar.Maximum = _raceManager.SelectedCar.FuelCapacity;
            RaceProgressBar.Value = _raceManager.GetRaceProgress() * 100;
        }

        private void AddToLog(string message)
        {
            var lines = ActionLogText.Text.Split('\n').Take(100);
            ActionLogText.Text = $"{DateTime.Now:T} - {message}\n{string.Join("\n", lines)}";
            ActionLogText.ScrollToHome();
        }

        private void EnableActionButtons(bool enabled)
        {
            SpeedUpButton.IsEnabled = enabled;
            MaintainButton.IsEnabled = enabled;
            PitStopButton.IsEnabled = enabled;
        }

        private void RaceManager_StateChanged(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(UpdateUI);
        }

        private void SpeedUpButton_Click(object sender, RoutedEventArgs e) => ExecuteAction(ActionType.SpeedUp);
        private void MaintainButton_Click(object sender, RoutedEventArgs e) => ExecuteAction(ActionType.Maintain);
        private void PitStopButton_Click(object sender, RoutedEventArgs e) => ExecuteAction(ActionType.PitStop);

        private void CarComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CarComboBox.SelectedItem is Car selectedCar && _raceManager.CurrentState == RaceState.NotStarted)
            {
                _raceManager.SelectCar(selectedCar);
                UpdateUI();
            }
        }
    }
}