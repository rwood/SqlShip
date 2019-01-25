using System.Timers;
using SqlShip.Interfaces;
using Timer = System.Timers.Timer;

namespace SqlShip.Services
{
    public class AutoUpdateService : IAutoUpdateService
    {
        private readonly IUpdaterService _updater;
        private readonly IUpdaterConfig _userSettings;
        private readonly Timer _timer;

        public AutoUpdateService(IUpdaterService updater, IUpdaterConfig userSettings)
        {
            _updater = updater;
            _userSettings = userSettings;
            _timer = new Timer(100) {AutoReset = false};
            _timer.Elapsed += TimerOnElapsed;
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private async void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            await _updater.DeleteMarkedFiles();
            if (await _updater.TryUpdate())
            {
                _updater.RestartApp();
            }
            else
            {
                _timer.Interval = _userSettings.UpdateIntervalCheckMinutes * 60 * 1000;
                _timer.Start();
            }
        }
    }
}
