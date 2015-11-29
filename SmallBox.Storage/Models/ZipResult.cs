using System.IO;

namespace SmallBox.Storage.Models
{
    public class ZipResult
    {
        public Stream Stream { get; set; }
        public string FullName { get; set; }
    }
}