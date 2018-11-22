using System.IO;
using System.IO.Compression;
using System;

namespace MastersOfTempest
{
    namespace Tools
    {
        public static class FileHandling
        {
            public static void Decompress(FileInfo fileToDecompress)
            {
                using (FileStream originalFileStream = fileToDecompress.OpenRead())
                {
                    string currentFileName = fileToDecompress.FullName;
                    string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                    using (FileStream decompressedFileStream = File.Create(newFileName))
                    {
                        using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                        {
                            decompressionStream.CopyTo(decompressedFileStream);
                        }
                    }
                }
            }

            public static byte[] ReadFile(FileInfo fileToRead, uint sizeBuffer)
            {
                byte[] buffer = new byte[sizeBuffer];
                using (FileStream originalFileStream = fileToRead.OpenRead())
                {
                    originalFileStream.Read(buffer, 0, buffer.Length);
                }
                return buffer;
            }

            public static void DeleteFile(String fileToDelete)
            {
                if (File.Exists(fileToDelete))
                {
                    File.Delete(fileToDelete);
                }
            }
        }
    }
}
