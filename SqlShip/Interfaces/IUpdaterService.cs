using System.Threading.Tasks;

namespace SqlShip.Interfaces
{
    public interface IUpdaterService
    {
        string WaitingBuild { get; }
        string CurrentBuild { get; }
        Task<bool> UpdateWaiting();
        Task<bool> TryUpdate();
        Task DeleteMarkedFiles();
        void RestartApp();
        string GetCurrentBuild();
    }
}