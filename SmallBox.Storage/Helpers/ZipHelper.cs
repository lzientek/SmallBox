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
            return zip.Entries.Select(file => new ZipResult() {Stream = file.Open(), FullName = file.FullName})
                .ToArray();
        }

        public static Stream Zip(List<ZipResult> files)
        {
            var stream = new MemoryStream();
            var archive = new ZipArchive(stream);
            foreach (var zipResult in files)
            {

               var entry = archive.CreateEntry(zipResult.FullName);
            }
            return stream;
        }
    }
}
