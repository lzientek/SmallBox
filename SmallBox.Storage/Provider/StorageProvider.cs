using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SmallBox.Storage.Helpers;
using SmallBox.Storage.Models;
using SmallBox.Storage.Models.FolderRes;

namespace SmallBox.Storage.Provider
{
    public class StorageProvider
    {
        public const string ContainerName = "smallboxcontainer";
        private const string ConnectionString = "StorageConnectionString";

        private CloudStorageAccount _storageAccount;
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _container;

        public StorageProvider()
        {
            Load();
        }

        public bool Load()
        {
            try
            {
                _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(ConnectionString));
                _blobClient = _storageAccount.CreateCloudBlobClient();
                _container = _blobClient.GetContainerReference(ContainerName);
                return _container.CreateIfNotExists();
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public FolderResult GetRootFolders()
        {
            return BlobsToFolderResult("root", _container.ListBlobs().OfType<CloudBlobDirectory>());
        }

        public async Task<List<IListBlobItem>> GetAllFiles()
        {
            BlobContinuationToken token = null;
            var blobs = new List<IListBlobItem>();
            do
            {
                BlobResultSegment resultSegment = await _container.ListBlobsSegmentedAsync(token);
                token = resultSegment.ContinuationToken;
                blobs.AddRange(resultSegment.Results);
            } while (token != null);

            return blobs;
        }

        /// <summary>
        /// Get a file list from a folder
        /// </summary>
        /// <param name="folderId">folder name</param>
        /// <returns></returns>
        public FolderResult GetFilesFromFolder(string folderId)
        {
            var directory = _container.GetDirectoryReference(folderId);
            if (directory == null)
            {
                return null;
            }

            var blobs = directory.ListBlobs();
            return BlobsToFolderResult(folderId, blobs);
        }

        private FolderResult BlobsToFolderResult(string folderId, IEnumerable<IListBlobItem> blobs)
        {
            var lst = new List<StorageObj>();
            foreach (var blob in blobs)
            {
                //if it's a folder
                var dir = blob as CloudBlobDirectory;
                if (dir != null)
                {
                    lst.Add(new StorageObj { Name = dir.Prefix, Type = StorageObj.FolderType });
                }
                else
                {
                    //if it's a file
                    var file = blob as CloudBlockBlob;
                    if (file != null)
                    {
                        lst.Add(new StorageObj { Name = file.Name, Type = StorageObj.FileType });
                    }
                }
            }

            return new FolderResult()
            {
                FolderName = folderId,
                Content = lst,
            };
        }

        /// <summary>
        /// Save file in container
        /// </summary>
        /// <param name="folderName">folder Name</param>
        /// <param name="fileName">File Name</param>
        /// <param name="data">File data</param>
        /// <param name="contentType"></param>
        /// <returns>Save is success</returns>
        public async Task<bool> SaveFile(string folderName, string fileName, Stream data, string contentType)
        {
            try
            {
                var extension = Path.GetExtension(fileName);
                if (extension != null && (extension.ToLower() == ".zip") || contentType == "application/x-zip-compressed")
                {
                    return await ExtractAndSaveZipFile(folderName, fileName, data);
                }
                var blob = _container.GetBlockBlobReference(Path.Combine(folderName, fileName));
                blob.Properties.ContentType = contentType;
                await blob.UploadFromStreamAsync(data);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> ExtractAndSaveZipFile(string folderName, string fileName, Stream data)
        {
            var result = ZipHelper.UnZip(data);
            folderName = Path.Combine(folderName, Path.GetFileNameWithoutExtension(fileName));
            foreach (var unZipResult in result.AsParallel())
            {
                var blob = _container.GetBlockBlobReference(Path.Combine(folderName, unZipResult.FullName));
                await blob.UploadFromStreamAsync(unZipResult.Stream);
            }
            return true;
        }

        /// <summary>
        /// Zip a folder and save it in the archive folder
        /// </summary>
        /// <param name="folderPath">name of the folder to zip</param>
        /// <returns>return file name and file size in a tuple</returns>
        public async Task<Tuple<string, long>> ZipFolder(string folderPath)
        {
            //download files to zip
            var folder = _container.GetDirectoryReference(folderPath);
            List<ZipResult> files = new List<ZipResult>();
            foreach (var listBlobItem in folder.ListBlobs(true).OfType<CloudBlockBlob>().AsParallel())
            {
                var stream = new MemoryStream();
                await listBlobItem.DownloadToStreamAsync(stream);
                files.Add(new ZipResult() { Stream = stream, FullName = listBlobItem.Name });
            }

            //save the files in a zip (stream)
            using (var zipStream = await ZipHelper.Zip(files))
            {
                //upload the zip to azure
                var archivesZipName = string.Format("{0}.zip", Path.Combine("Archives", folderPath.Replace('/', '_').Replace('\\', '_')));
                var archivesZipBlob = _container.GetBlockBlobReference(archivesZipName);
                await archivesZipBlob.UploadFromStreamAsync(zipStream);
                return new Tuple<string, long>(Path.GetFileName(archivesZipName), zipStream.Length);
            }
        }



        /// <summary>
        /// Get byte array from a file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task<FileToDownload> GetFileStream(string filePath)
        {
            var blob = _container.GetBlockBlobReference(filePath);
            MemoryStream value = new MemoryStream();
            await blob.DownloadToStreamAsync(value);
            value.Position = 0;
            return new FileToDownload() { Data = value, ContentType = blob.Properties.ContentType };
        }
    }
}
