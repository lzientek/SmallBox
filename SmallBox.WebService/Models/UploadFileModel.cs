using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace SmallBox.WebService.Models
{

    [DataContract]
    public class UploadFileModel
    {
        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public long Length { get; set; }
    }
}