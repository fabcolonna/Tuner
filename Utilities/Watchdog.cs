using System;
using System.Timers;

namespace TunerWinUI.Utilities
{
    public class Watchdog
    {
        private readonly Timer mTimer;

        public event EventHandler OnTriggered;

        public event EventHandler OnStopped;

        public bool IsCountingDown { get; private set; }

        public bool AutoRestartAfterTriggered { get; set; } = true;

        public Watchdog(int millis, bool startImmediately)
        {
            if (millis < 0)
                throw new ArgumentOutOfRangeException(nameof(millis) + " can't be < 0");

            mTimer = new Timer(millis);
            mTimer.Elapsed += (sender, args) =>
            {
                IsCountingDown = false;
                OnTriggered?.Invoke(this, EventArgs.Empty);
                if (AutoRestartAfterTriggered)
                    Restart();
            };

            if (startImmediately)
                Start();
        }

        public void Start()
        {
            IsCountingDown = true;
            mTimer.Start();
        }

        public void Stop()
        {
            if (!IsCountingDown) return;
            IsCountingDown = false;
            mTimer.Stop();
            OnStopped?.Invoke(this, EventArgs.Empty);
        }

        public void Restart()
        {
            Stop();
            Start();
        }


    }
}
