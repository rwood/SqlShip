using System;

namespace SqlShip.Logging
{
    public class LogMessage
    {
        public DateTimeOffset Date { get; set; }
        public Exception Exception { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public object[] Parameters { get; set; }
    }
}