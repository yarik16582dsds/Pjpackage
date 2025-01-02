using System.Collections.Generic;

namespace PjPackageLibrary.DataStructures
{
    public class PjPackageHeader
    {
        public string MagicNumber { get; set; } = "PJPG";
        public int Version { get; set; } = 1;
    }

    public class PjPackageMetadata
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }

    public class PjPackageFileEntry
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public byte[] Data { get; set; }
    }

    public class PjPackage
    {
        public PjPackageHeader Header { get; set; } = new PjPackageHeader();
        public PjPackageMetadata Metadata { get; set; } = new PjPackageMetadata();
        public List<PjPackageFileEntry> Files { get; set; } = new List<PjPackageFileEntry>();
    }
}
