using System;
using System.Collections.Concurrent;
using System.Timers;

namespace SqlShip.Logging
{
    public sealed class LogBroker
    {
        public event EventHandler<LogMessage> LogMessageReceived;
        private readonly Timer _timer;
        public LogBroker()
        {
            _timer = new Timer
            {
                AutoReset = true,
                Interval = 500
            };
            _timer.Elapsed += TimerOnElapsed;
            _timer.Enabled = true;
            _timer.AutoReset = false;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                if (_messages.TryDequeue(out var message))
                {
                    OnLogMessageReceived(message);
                }
            }
            finally
            {
                _timer.Start();
            }
        }

        private readonly ConcurrentQueue<LogMessage> _messages = new ConcurrentQueue<LogMessage>();

        public void Log(LogMessage message)
        {
            if(message == null) return;
            _messages.Enqueue(message);
        }

        private void OnLogMessageReceived(LogMessage e)
        {
            LogMessageReceived?.Invoke(this, e);
        }
    }
}