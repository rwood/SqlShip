using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using HtmlAgilityPack;
using SqlShip.Helpers;
using SqlShip.Interfaces;

namespace SqlShip.Services
{
    public class HttpUpdaterService : IUpdaterService
    {
#if DEBUG
        private const string BuildType = "Debug";
#else
        private const string BuildType = "Release";
#endif
        private readonly ILogger _logger;
        private readonly IUpdaterConfig _userSettings;
        public HttpUpdaterService(ILogger logger, IUpdaterConfig userSettings)
        {
            _userSettings = userSettings;
            CurrentBuild = GetCurrentBuild();
            WaitingBuild = CurrentBuild;
            _logger = logger;
        }

        public string CurrentBuild { get; }
        private readonly object _syncLock = new object();
        public string WaitingBuild { get; private set; }
        public async Task DeleteMarkedFiles()
        {
            foreach (var file in Directory.GetFiles(".", "*.delete"))
            {
                _logger.Information("Deleting: {0}", file);
                for (var retry = 0; retry < 3; retry++)
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        if (retry < 3)
                        {
                            _logger.Warning(ex, "Error while deleting file {0}. Retrying...", file);
                            await Task.Delay(1000);
                        }
                        else
                        {
                            _logger.Error(ex, "Could not delete file {0}", file);
                        }
                    }
            }

            _logger.Information("Deleted all files");
        }

        public string GetCurrentBuild()
        {
            return Path.GetFileName(Directory.GetFiles(Environment.CurrentDirectory, $"{BuildType}_*.zip")
                .OrderByDescending(f => f).FirstOrDefault());
        }

        private static string GetMostRecentBuild(string source)
        {
            var html = new HtmlDocument();
            html.LoadHtml(source);
            var root = html.DocumentNode;
            return root.Descendants("a")
                .Select(n => n.GetAttributeValue("href", ""))
                .Where(n => n.EndsWith(".zip") && n.StartsWith(BuildType))
                .OrderByDescending(s => s).FirstOrDefault();
        }

        public void RestartApp()
        {
            _logger.Information("Restarting");
            switch (GlobalConfigs.ProcessState)
            {
                case ProcessState.WinForm:
                {
                    var appPath = new Uri(Assembly.GetEntryAssembly().CodeBase);
                    var info = new ProcessStartInfo(appPath.LocalPath);
                    Process.Start(info);
                    Environment.Exit(0);
                    break;
                }
                case ProcessState.Console:
                {
                    var appPath = new Uri(Assembly.GetEntryAssembly().CodeBase);
                    var info = new ProcessStartInfo("cmd.exe")
                    {
                        Arguments = $"/C \"{appPath.LocalPath}\"",
                        UseShellExecute = true,
                        CreateNoWindow = false,
                    };
                    Process.Start(info);
                    Environment.Exit(0);
                    break;
                }
                case ProcessState.Service:
                    Environment.Exit(0);
                    break;
            }
        }

        public async Task<bool> TryUpdate()
        {
            if (!await UpdateWaiting()) return false;
            lock (_syncLock)
            {
                if (WaitingBuild == null) return false;
                _logger.Information($"Updating to {WaitingBuild}");
                new WebClient().DownloadFile($"{_userSettings.UpdateUrl}/{WaitingBuild}", WaitingBuild);
                foreach (var file in Directory.GetFiles(".", "*.*"))
                {
                    if (file.EndsWith(WaitingBuild) || file.EndsWith(".config") || file.Contains(".vshost")) continue;
                    File.Move(file, $"{file}.delete");
                }

                ZipFile.ExtractToDirectory(WaitingBuild, Environment.CurrentDirectory);
                _logger.Information($"Updated to {WaitingBuild}");
                return true;
            }
        }

        public async Task<bool> UpdateWaiting()
        {
#if DEBUG
            return false;
#endif
            try
            {
                var client = new WebClient();
                var content = await Task.Run(() => client.DownloadString(_userSettings.UpdateUrl));
                WaitingBuild = GetMostRecentBuild(content);
                _logger.Information($"Most recent build: {WaitingBuild}");
                if (!File.Exists(WaitingBuild)) return true;
                _logger.Information("Already up to date");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while checking for service updates.");
                return false;
            }

            return false;

        }
    }
}