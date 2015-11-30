using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmallBox.Storage.Models;

namespace SmallBox.Storage.Helpers
{
    public class ZipHelper
    {
        public static ZipResult[] UnZip(Stream stream)
        {
            var zip = new ZipArchive(stream, ZipArchiveMode.Read);
            return zip.Entries.Select(file => new ZipResult() { Stream = file.Open(), FullName = file.FullName })
                .ToArray();
        }

        public static async Task<MemoryStream> Zip(List<ZipResult> files)
        {
            var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                foreach (var zipResult in files)
                {
                    var entry = archive.CreateEntry(zipResult.FullName);
                    using (var writeStream = entry.Open())
                    {
                        int readSize;
                        var buffer = new byte[8192];

                        //read stream and dispose it
                        zipResult.Stream.Position = 0;
                        while ((readSize = await zipResult.Stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await writeStream.WriteAsync(buffer, 0, readSize);
                        }
                        zipResult.Stream.Dispose();
                    }
                }
            }
            stream.Position = 0;
            return stream;
        }
    }
}
