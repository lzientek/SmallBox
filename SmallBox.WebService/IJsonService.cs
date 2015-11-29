using System.IO;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using SmallBox.Storage.Models.FolderRes;
using SmallBox.WebService.Models;

namespace SmallBox.WebService
{
    [ServiceContract]
    public interface IJsonService
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "root/")]
        FolderResult ListRoot();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "folders/{*folderPath}")]
        FolderResult ListFolder(string folderPath);

        [OperationContract(Name = "Upload")]
        [DataContractFormat]
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "file/{*filePath}")]
        Task<UploadFileModel> UploadInFolder(string filePath, Stream file);

        [OperationContract]
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "zip/{*folderPath}")]
        void ZipFolder(string folderPath);


        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "zip/{*zipPath}")]
        Task<Stream> DownloadZip(string zipPath);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "file/{*filePath}")]
        Task<Stream> DownloadFile(string filePath);
    }
}
