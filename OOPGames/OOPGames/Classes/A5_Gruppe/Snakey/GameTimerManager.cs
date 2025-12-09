using System;
using System.Windows.Threading;

namespace OOPGames
{

    /// Verwaltet Game-Timer und Countdown

    public class GameTimerManager
    {
        private readonly DispatcherTimer _gameTimer;
        private readonly DispatcherTimer _countdownTimer;
        private readonly Action _onGameTick;
        private readonly Action _onCountdownTick;

        public int CountdownSeconds { get; private set; }
        public bool IsCountingDown { get; private set; }
        public bool IsGameRunning { get; private set; }

        public GameTimerManager(int timerIntervalMs, Action onGameTick, Action onCountdownTick)
        {
            _onGameTick = onGameTick;
            _onCountdownTick = onCountdownTick;

            _gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(timerIntervalMs)
            };
            _gameTimer.Tick += OnGameTimerTick;

            _countdownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _countdownTimer.Tick += OnCountdownTimerTick;
        }

        public void StartCountdown(int seconds)
        {
            if (IsGameRunning || IsCountingDown) return;
            CountdownSeconds = seconds;
            IsCountingDown = true;
            _countdownTimer.Start();
        }

        public void StopGameTimer()
        {
            _gameTimer.Stop();
            IsGameRunning = false;
        }

        private void OnGameTimerTick(object sender, EventArgs e)
        {
            if (!IsCountingDown)
            {
                _onGameTick?.Invoke();
            }
        }

        private void OnCountdownTimerTick(object sender, EventArgs e)
        {
            CountdownSeconds--;
            _onCountdownTick?.Invoke();

            if (CountdownSeconds <= 0)
            {
                _countdownTimer.Stop();
                IsCountingDown = false;
                _gameTimer.Start();
                IsGameRunning = true;
            }
        }
    }
}
