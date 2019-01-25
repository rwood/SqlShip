using System.Text;

namespace SqlShip.Logging
{
    public abstract class LogConsumerBase
    {
        private LogLevel _currentLogLevel;

        protected LogConsumerBase(LogBroker broker)
        {
            broker.LogMessageReceived += BrokerOnLogMessageReceived;
        }

        private void BrokerOnLogMessageReceived(object sender, LogMessage logMessage)
        {
            if (_currentLogLevel <= logMessage.Level) HandleMessage(logMessage);
        }

        public void Subscribe(LogLevel level)
        {
            _currentLogLevel = level;
        }

        protected abstract void HandleMessage(LogMessage message);

        protected static string Format(LogMessage message)
        {
            if (message.Level == LogLevel.None)
                return "";
            const string format = "[{0, 7}] {1, 30} {2}";

            var sb = new StringBuilder();

            sb.AppendFormat(format, message.Level, message.Date.ToString("o"),
                string.Format(message.Message, message.Parameters));

            if (message.Exception == null) return sb.ToString();
            sb.AppendLine();
            sb.AppendLine(message.Exception.Message);
            sb.Append(message.Exception.StackTrace);

            return sb.ToString();
        }
    }
}