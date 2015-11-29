using System.IO;

namespace SmallBox.Storage.Models
{
    public class FileToDownload
    {
        public Stream Data { get; set; }
        public string ContentType { get; set; }
    }
}