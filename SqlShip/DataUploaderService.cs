using SqlShip.Helpers;
using SqlShip.Interfaces;
using Topshelf;
using Topshelf.Hosts;

namespace SqlShip
{
    public class DataUploaderService
    {
        private readonly IAutoUpdateService _autoUpdate;
        private readonly HighWaterMarkQueryService _highWaterMarkQueryService;

        public DataUploaderService(IAutoUpdateService autoUpdateService, HighWaterMarkQueryService highWaterMarkQueryService)
        {
            _autoUpdate = autoUpdateService;
            _highWaterMarkQueryService = highWaterMarkQueryService;
        }

        public bool Start(HostControl control)
        {
            GlobalConfigs.ProcessState = control is ConsoleRunHost ? ProcessState.Console : ProcessState.Service;
            _autoUpdate.Start();
            _highWaterMarkQueryService.Start();
            return true;
        }

        public void Stop()
        {
            _autoUpdate.Stop();
            _highWaterMarkQueryService.Stop();
        }
    }
}
