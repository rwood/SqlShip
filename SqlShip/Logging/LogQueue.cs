using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SqlShip.Logging
{
    public class LogQueue : LogConsumerBase
    {
        private readonly AutoResetEvent _messageReceived = new AutoResetEvent(false);

        public LogQueue(LogBroker broker) : base(broker)
        {
        }

        private ConcurrentQueue<LogMessage> LogMessages { get; } = new ConcurrentQueue<LogMessage>();

        protected override void HandleMessage(LogMessage message)
        {
            LogMessages.Enqueue(message);
            OnLogMessageReceived(message);
            _messageReceived.Set();
        }

        private LogMessage GetNextMessage()
        {
            return LogMessages.TryDequeue(out var message) ? message : null;
        }

        public LogMessage WaitForNextMessage(TimeSpan? timeout = null)
        {
            if (timeout == null)
                timeout = TimeSpan.Zero;
            return _messageReceived.WaitOne(timeout.Value) ? GetNextMessage() : null;
        }

        public event EventHandler<LogMessage> LogMessageReceived;

        protected virtual void OnLogMessageReceived(LogMessage e)
        {
            LogMessageReceived?.Invoke(this, e);
        }
    }
}