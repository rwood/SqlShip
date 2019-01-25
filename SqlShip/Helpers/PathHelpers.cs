using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SqlShip.Helpers
{
    public static class PathHelpers
    {
        private const int FileAttributeDirectory = 0x10;
        private const int FileAttributeNormal = 0x80;

        public static string GetRelativePath(string fromPath, string toPath)
        {
            var fromAttr = GetPathAttribute(fromPath);
            var toAttr = GetPathAttribute(toPath);

            var path = new StringBuilder(260); // MAX_PATH
            if (PathRelativePathTo(
                    path,
                    fromPath,
                    fromAttr,
                    toPath,
                    toAttr) ==
                0)
                throw new ArgumentException("Paths must have a common prefix");
            return path.ToString().Trim('.', '\\');
        }

        private static int GetPathAttribute(string path)
        {
            var di = new DirectoryInfo(path);
            if (di.Exists)
                return FileAttributeDirectory;

            var fi = new FileInfo(path);
            if (fi.Exists)
                return FileAttributeNormal;

            throw new FileNotFoundException();
        }

        [DllImport("shlwapi.dll", SetLastError = true)]
        private static extern int PathRelativePathTo(
            StringBuilder pszPath,
            string pszFrom,
            int dwAttrFrom,
            string pszTo,
            int dwAttrTo);
    }
}
