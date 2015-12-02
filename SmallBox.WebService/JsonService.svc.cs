using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using SmallBox.Storage.Models.FolderRes;
using SmallBox.Storage.Provider;
using SmallBox.WebService.Models;

namespace SmallBox.WebService
{
    public class JsonService : IJsonService
    {
        private readonly StorageProvider _provider;
        public JsonService()
        {
            _provider = new StorageProvider();
        }

        /// <summary>
        /// List the folders of the root folder
        /// </summary>
        /// <returns></returns>
        public FolderResult ListRoot()
        {
            return _provider.GetRootFolders();
        }

        /// <summary>
        /// list the content of the folder
        /// </summary>
        /// <param name="folderPath">folder name where list files and folders</param>
        /// <returns></returns>
        public FolderResult ListFolder(string folderPath)
        {
            try
            {
                return _provider.GetFilesFromFolder(folderPath);
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        /// <summary>
        /// Upload a file in a folder
        /// </summary>
        /// <param name="filePath">folder name and file name</param>
        /// <param name="file">file data</param>
        /// <returns>result</returns>
        public async Task<UploadFileModel> UploadInFolder(string filePath, Stream file)
        {
            var webOperationContext = WebOperationContext.Current.IncomingRequest;
            //verify folder and file name
            var fileName = Path.GetFileName(filePath);
            if (fileName == null) { throw new FaultException("Invalid path, the path doesn't contains any file name."); }

            var path = filePath.Replace(fileName, "");
            if (string.IsNullOrWhiteSpace(path) || path == "/" || path == @"\")
            {
                throw new FaultException("Invalid upload folder, (a file cannot be saved in the root folder).");
            }

            //upload the file to Azure
            var result = await _provider.SaveFile(path, fileName, file, webOperationContext.ContentType);
            if (!result) { throw new FaultException("Unexpected Error during the upload of the file."); }

            //create the return object
            var upload = new UploadFileModel { FilePath = path, FileName = fileName, Length = webOperationContext.ContentLength };
            return upload;
        }

        /// <summary>
        /// zip a folder and save it the Archives root folder
        /// </summary>
        /// <param name="folderPath">folder name to zip</param>
        public async Task<UploadFileModel> ZipFolder(string folderPath)
        {
            var result = await _provider.ZipFolder(folderPath);
            return new UploadFileModel
            {
                FilePath = "Archives/",
                FileName = result.Item1,
                Length = result.Item2
            };
        }

        /// <summary>
        /// Download a zip file from the archives folder
        /// </summary>
        /// <param name="zipPath">zip name or path</param>
        /// <returns>zip file</returns>
        public async Task<Stream> DownloadZip(string zipPath)
        {
            if (Path.GetExtension(zipPath) != ".zip")
            {
                throw new FaultException("Not a valid file to download");
            }
            var path = zipPath.TrimStart('/', '\\');
            if (!path.StartsWith("Archives"))
            {
                path = Path.Combine("Archives", path);
            }

            return await Download(path, WebOperationContext.Current.OutgoingResponse);
        }


        /// <summary>
        /// Download a file by it's path
        /// </summary>
        /// <param name="filePath">path from the file to download</param>
        /// <returns>file</returns>
        public async Task<Stream> DownloadFile(string filePath)
        {
            return await Download(filePath, WebOperationContext.Current.OutgoingResponse);
        }

        /// <summary>
        /// Generate a download result
        /// </summary>
        /// <param name="filePath">file to download</param>
        /// <param name="outgoingResponse"></param>
        /// <returns>file</returns>
        private async Task<Stream> Download(string filePath, OutgoingWebResponseContext outgoingResponse)
        {
            var file = await _provider.GetFileStream(filePath);
            string headerInfo = string.Format("attachment; filename={0}", Path.GetFileName(filePath));
            if (outgoingResponse != null)
            {
                outgoingResponse.Headers["Content-Disposition"] = headerInfo;
                outgoingResponse.ContentType = file.ContentType ?? "application/octet-stream";
            }
            return file.Data;
        }
    }
}
