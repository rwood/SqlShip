using System;
using SqlShip.Interfaces;

namespace SqlShip.Logging
{
    public class Logger : ILogger
    {
        private readonly LogBroker _broker;

        public Logger(LogBroker broker)
        {
            _broker = broker;
        }

        public void Critical(Exception ex, string message, params object[] parameters)
        {
            Log(LogLevel.Critical, message, parameters, ex);
        }

        public void Critical(string message, params object[] parameters)
        {
            Log(LogLevel.Critical, message, parameters);
        }

        public void Debug(string message, params object[] parameters)
        {
            Log(LogLevel.Debug, message, parameters);
        }

        public void Error(Exception ex, string message, params object[] parameters)
        {
            Log(LogLevel.Error, message, parameters, ex);
        }
        public void Error(string message, params object[] parameters)
        {
            Log(LogLevel.Error, message, parameters);
        }

        public void Information(string message, params object[] parameters)
        {
            Log(LogLevel.Information, message, parameters);
        }

        public void Trace(string message, params object[] parameters)
        {
            Log(LogLevel.Trace, message, parameters);
        }

        public void Warning(Exception ex, string message, params object[] parameters)
        {
            Log(LogLevel.Warning, message, parameters, ex);
        }

        public void Warning(string message, params object[] parameters)
        {
            Log(LogLevel.Warning, message, parameters);
        }

        private void Log(LogLevel level, string message, object[] parameters, Exception ex = null)
        {
            var logMessage = new LogMessage()
            {
                Level = level,
                Message = message,
                Exception = ex,
                Parameters = parameters,
                Date = DateTimeOffset.Now
            };
            _broker.Log(logMessage);
        }
    }
}