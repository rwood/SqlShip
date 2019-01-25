using System.IO;

namespace SqlShip.Interfaces
{
    public interface IUpdatePackagerConfig
    {
        DirectoryInfo SourceDirectory { get; set; }
        DirectoryInfo DestinationDirectory { get; set; }
    }
}