using System.Collections.Generic;

namespace SmallBox.Storage.Models.FolderRes
{
    public class FolderResult
    {
        public string FolderName { get; set; }

        public List<StorageObj> Content { get; set; }
    }
}
