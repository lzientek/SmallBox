using System.Runtime.Serialization;

namespace SmallBox.WebService.Models
{
    [DataContract]

    public class FaultModel

    {

        [DataMember]
        public string FaultMessage { get; set; }

        [DataMember]
        public int ErrorCode { get; set; }
    }
}