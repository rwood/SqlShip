using System;

namespace SqlShip.Logging
{
    public class LogConsoleWriter : LogConsumerBase
    {
        public LogConsoleWriter(LogBroker broker) : base(broker)
        {
        }

        protected override void HandleMessage(LogMessage message)
        {
            Console.WriteLine(message);
        }
    }
}
