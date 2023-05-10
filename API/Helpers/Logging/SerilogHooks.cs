using Serilog.Sinks.File.Archive;
using System.IO.Compression;

namespace API.Helpers.Logging
{
    public class SerilogHooks
    {
        public static ArchiveHooks MyArchiveHooks =>
            new ArchiveHooks(CompressionLevel.Fastest, "D:\\Archive\\{UtcDate:yyyy}\\{UtcDate:MM}");
    }
}
