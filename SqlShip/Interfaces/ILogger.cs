using System;

namespace SqlShip.Interfaces
{
    public interface ILogger
    {
        void Critical(Exception ex, string message, params object[] parameters);
        void Critical(string message, params object[] parameters);
        void Debug(string message, params object[] parameters);
        void Error(string message, params object[] parameters);
        void Error(Exception ex, string message, params object[] parameters);
        void Information(string message, params object[] parameters);
        void Trace(string message, params object[] parameters);
        void Warning(string message, params object[] parameters);
        void Warning(Exception ex, string message, params object[] parameters);
    }
}