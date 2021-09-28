using System.IO;
using System.IO.Compression;
using System.Text;

namespace Wikiled.News.Monitoring.Extensions
{
    public static class CompressionExtension
    {
        public static byte[] Zip(this string textToZip, string fileName, Encoding encoding)
        {
            using MemoryStream memoryStream = new MemoryStream();
            using (ZipArchive zipArchive = new ZipArchive((Stream)memoryStream, ZipArchiveMode.Create, true, encoding))
            {
                using Stream stream = zipArchive.CreateEntry(fileName).Open();
                using StreamWriter streamWriter = new StreamWriter(stream, encoding);
                streamWriter.Write(textToZip);
            }

            return memoryStream.ToArray();
        }
    }
}
