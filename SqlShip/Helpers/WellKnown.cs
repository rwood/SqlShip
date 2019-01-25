using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SqlShip.Helpers
{
    public static class WellKnown
    {
        public static string GetDataDirectory()
        {
            var current = new FileInfo(Assembly.GetEntryAssembly().Location);
            Debug.Assert(current.Directory != null, "current.Directory != null");
            var dataDir = Path.Combine(current.Directory.FullName, ".data");
            if (!Directory.Exists(dataDir))
                Directory.CreateDirectory(dataDir);
            return dataDir;
        }
    }
}
