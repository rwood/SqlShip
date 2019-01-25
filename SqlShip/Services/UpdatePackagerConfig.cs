using System.IO;
using SqlShip.Interfaces;

namespace SqlShip.Services
{
    public class UpdatePackagerConfig : IUpdatePackagerConfig
    {
        public DirectoryInfo SourceDirectory { get; set; }
        public DirectoryInfo DestinationDirectory { get; set; }
    }
}