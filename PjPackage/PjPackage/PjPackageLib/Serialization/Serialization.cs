using System.IO;
using System.Collections.Generic;
using PjPackageLibrary.DataStructures;

namespace PjPackageLibrary.Serialization
{
    public static class PjPackageSerializer
    {
        public static void Serialize(PjPackage package, string filePath)
        {
            using (var writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                writer.Write(package.Header.MagicNumber);
                writer.Write(package.Header.Version);
                writer.Write(package.Metadata.Name);
                writer.Write(package.Metadata.Version);
                writer.Write(package.Files.Count);

                foreach (var file in package.Files)
                {
                    writer.Write(file.FileName);
                    writer.Write(file.FileSize);
                    writer.Write(file.Data);
                }
            }
        }

        public static PjPackage Deserialize(string filePath)
        {
            using (var reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                PjPackage package = new PjPackage();
                package.Header.MagicNumber = reader.ReadString();
                package.Header.Version = reader.ReadInt32();
                package.Metadata.Name = reader.ReadString();
                package.Metadata.Version = reader.ReadString();
                int fileCount = reader.ReadInt32();

                package.Files = new List<PjPackageFileEntry>();
                for (int i = 0; i < fileCount; i++)
                {
                    string fileName = reader.ReadString();
                    long fileSize = reader.ReadInt64();
                    byte[] fileData = reader.ReadBytes((int)fileSize);
                    package.Files.Add(new PjPackageFileEntry { FileName = fileName, FileSize = fileSize, Data = fileData });
                }
                return package;
            }
        }
    }
}
