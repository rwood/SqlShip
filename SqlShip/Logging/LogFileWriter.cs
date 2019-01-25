using System;
using System.IO;

namespace SqlShip.Logging
{
    public class LogFileWriter : LogConsumerBase
    {
        public LogFileWriter(LogBroker broker) : base(broker)
        {
            this._currentFile = GenFileInfo();
        }

        protected override void HandleMessage(LogMessage message)
        {
            var formattedMessage = Format(message);
            if (formattedMessage == "") return;
            _currentFile.Refresh();
            if (!_currentFile.Exists || _currentFile.Length > FileByteThreshold)
            {
                _currentFile = GenFileInfo();
            }

            File.AppendAllLines(_currentFile.FullName, new[] { formattedMessage });
        }

        private const int FileByteThreshold = 10 * 1024 * 1024;

        private FileInfo _currentFile;

        private static FileInfo GenFileInfo()
        {
            if (!Directory.Exists("Logs")) Directory.CreateDirectory("Logs");
            var path = Path.Combine("Logs", $"{DateTime.Now.Ticks}.log.txt");
            var file = new FileInfo(path);
            if (!file.Exists) file.Create().Close();
            file.Refresh();
            return file;
        }

        
    }
}
