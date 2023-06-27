using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc.Html;

namespace GoogleDriveMaster_API.Models
{
    public class GoogleDriveFilesRepository
    {
        public string[] Scopes = { Google.Apis.Drive.v3.DriveService.Scope.Drive };

        //create Drive API service.
        //----------------------------------------------------------------------------------------------------------------------------------------
        public string CSPath { get; set; }
        public string ClientCode { get; set; }
        public string FolderName { get; set; }
        public string fileName { get; set; }
        public string files { get; set; }
        //----------------------------------------------------------------------------------------------------------------------------------------      

        public GoogleDriveFilesRepository([FromBody] DriveFilterBodyResponse sClientCode)
        {
            this.ClientCode = sClientCode.ClientCode;
            this.CSPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/" + ClientCode + "/");
        }
        public Google.Apis.Drive.v3.DriveService GetService()
        {

            //get Credentials from client_secret.json file 
            UserCredential credential;


            using (var stream = new FileStream(Path.Combine(CSPath, "client_secret.json"), FileMode.Open, FileAccess.Read))
            {
                // String FolderPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/"); 
                String FilePath = Path.Combine(CSPath, "DriveServiceCredentials.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(FilePath, true)).Result;

            }

            //create Drive API service.
            Google.Apis.Drive.v3.DriveService service = new Google.Apis.Drive.v3.DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "GoogleDriveRestAPI-v3",
            });
            return service;
        }

        public Google.Apis.Drive.v2.DriveService GetService_v2()
        {
            UserCredential credential;
            //var CSPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/");

            using (var stream = new FileStream(Path.Combine(CSPath, "client_secret.json"), FileMode.Open, FileAccess.Read))
            {

                String FilePath = Path.Combine(CSPath, "DriveServiceCredentials.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(FilePath, true)).Result;
            }

            //Create Drive API service.
            Google.Apis.Drive.v2.DriveService service = new Google.Apis.Drive.v2.DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "GoogleDriveRestAPI-v2",
            });
            return service;
        }


        public byte[] GetDriveFileByName(DriveFilterBodyResponse sfileName)
        {
            Google.Apis.Drive.v3.DriveService service = GetService();

            // Define parameters of request.
            this.fileName = sfileName.fileName;
            Google.Apis.Drive.v3.FilesResource.ListRequest FileListRequest = service.Files.List();
            //FileListRequest.Q = $"name = '{fileName}' and trashed = false";
            FileListRequest.Q = $"name contains '{Path.GetFileNameWithoutExtension(fileName)}' and trashed = false";
            FileListRequest.Fields = "files(id, name)";

            //List files. 
            IList<Google.Apis.Drive.v3.Data.File> files = FileListRequest.Execute().Files;
            if (files != null && files.Count > 0)
            {
                var file = files.FirstOrDefault();
                var fileId = file.Id;
                var fileMetadata = service.Files.Get(fileId).Execute();

                using (var stream = new MemoryStream())
                {
                    service.Files.Get(fileId).Download(stream);
                    return stream.ToArray();
                }
            }
            else
            {
                return null;
            }
        }

        //file Upload to the Google Drive root folder.
        public List<GoogleDriveFile> FileUpload(DriveFilterBodyResponse myObject)
        {
            var httpRequest = HttpContext.Current.Request;
            var files = httpRequest.Files;

            if (files.Count > 0)
            {
                Google.Apis.Drive.v3.DriveService service = GetService();

                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    
                    if (file != null && file.ContentLength > 0)
                    {
                        var FileMetaData = new Google.Apis.Drive.v3.Data.File();
                        FileMetaData.Name = Path.GetFileName(file.FileName);
                        FileMetaData.MimeType = file.ContentType;

                        Google.Apis.Drive.v3.FilesResource.CreateMediaUpload request;

                        using (var stream = file.InputStream)
                        {
                            request = service.Files.Create(FileMetaData, stream, FileMetaData.MimeType);
                            request.Fields = "id";
                            request.Upload();
                        }
                    }
                }
            }
           
            List<GoogleDriveFile> FileList = new List<GoogleDriveFile>();
             return FileList;
        }
        //listofuploadedfiles.Text += String.Format("{0}<br />", uploadedFile.FileName);


        public List<GoogleDriveFile> CreateFolder([FromBody] DriveFilterBodyResponse sFolderName)
        {
            this.FolderName = sFolderName.FolderName;
            Google.Apis.Drive.v3.DriveService service = GetService();

            var FileMetaData = new Google.Apis.Drive.v3.Data.File();
            FileMetaData.Name = FolderName.ToString();
            FileMetaData.MimeType = "application/vnd.google-apps.folder";

            Google.Apis.Drive.v3.FilesResource.CreateRequest request;

            request = service.Files.Create(FileMetaData);
            request.Fields = "id, name, size, version, createdTime, parents, mimeType";
            var file = request.Execute();
            Console.WriteLine("Folder ID: " + file.Id);

            Google.Apis.Drive.v3.FilesResource.ListRequest FileListRequest = service.Files.List();
            //FileListRequest.Fields = "files(*)";
            IList<Google.Apis.Drive.v3.Data.File> files = FileListRequest.Execute().Files;
            List<GoogleDriveFile> FileList = new List<GoogleDriveFile>();
            GoogleDriveFile File = new GoogleDriveFile
            {
                Id = file.Id,
                Name = file.Name,
                Size = file.Size,
                Version = file.Version,
                CreatedTime = file.CreatedTime,
                Parents = file.Parents,
                MimeType = file.MimeType
            };
            FileList.Add(File);

            return FileList;
        }

        public List<GoogleDriveFile> FileUploadInFolder(string FolderName)
        {
            var httpRequest = HttpContext.Current.Request;
            var files = httpRequest.Files;
            if (files.Count > 0)
            {
                Google.Apis.Drive.v3.DriveService service = GetService();

                // Get the folderId of the folder with the given folderName
                var folderQuery = service.Files.List();
                folderQuery.Q = $"mimeType='application/vnd.google-apps.folder' and trashed=false and name='{FolderName}'";
                folderQuery.Fields = "nextPageToken, files(id)";
                var folderResult = folderQuery.Execute();
                var folderId = folderResult.Files.FirstOrDefault()?.Id;

                if (folderId != null)
                {
                    List<GoogleDriveFile> uploadedFiles = new List<GoogleDriveFile>();
                    for (int i = 0; i < files.Count; i++)
                    {
                        var file = files[i];
                        if (file != null && file.ContentLength > 0)
                        {
                            var FileMetaData = new Google.Apis.Drive.v3.Data.File()
                            {
                                Name = Path.GetFileName(file.FileName),
                                MimeType = file.ContentType,
                                Parents = new List<string> { folderId }
                            };
                            Google.Apis.Drive.v3.FilesResource.CreateMediaUpload request;
                            using (var stream = file.InputStream)
                            {
                                request = service.Files.Create(FileMetaData, stream, FileMetaData.MimeType);
                                request.Fields = "id, name";
                                request.Upload();
                            }
                            var fileResult = request.ResponseBody;
                            uploadedFiles.Add(new GoogleDriveFile() { Id = fileResult.Id, Name = fileResult.Name });
                        }
                    }
                    return uploadedFiles;
                }
            }
            return new List<GoogleDriveFile>();
        }


        // file save to server path
        private void SaveStream(MemoryStream stream, string FilePath)
        {
            using (System.IO.FileStream file = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite))
            {
                stream.WriteTo(file);
            }
        }


        public string DownloadGoogleFile(string fileName)
        {
            Google.Apis.Drive.v3.DriveService service = GetService();

            string folderPath = System.Web.HttpContext.Current.Server.MapPath("/GoogleDriveFiles/");
            Google.Apis.Drive.v3.FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.Q = $"name contains '{Path.GetFileNameWithoutExtension(fileName)}' and trashed = false";
            listRequest.Fields = "files(id, name)";
            //FileListRequest.Q = $"name contains '{Path.GetFileNameWithoutExtension(fileName)}' and trashed = false";
            // Get the list of files matching the file name
            var files = listRequest.Execute().Files;
            if (files == null || files.Count == 0)
            {
                throw new Exception($"File with name '{fileName}' not found.");
            }

            var file = files.First();

            string filePath = System.IO.Path.Combine(folderPath, file.Name);

            MemoryStream stream1 = new MemoryStream();

            // Add a handler which will be notified on progress changes.
            // It will notify on each chunk download and when the
            // download is completed or failed.
            var request = service.Files.Get(file.Id);
            request.MediaDownloader.ProgressChanged += (Google.Apis.Download.IDownloadProgress progress) =>
            {
                switch (progress.Status)
                {
                    case DownloadStatus.Downloading:
                        {
                            Console.WriteLine(progress.BytesDownloaded);
                            break;
                        }
                    case DownloadStatus.Completed:
                        {
                            Console.WriteLine("Download complete.");
                            SaveStream(stream1, filePath);
                            break;
                        }
                    case DownloadStatus.Failed:
                        {
                            Console.WriteLine("Download failed.");
                            break;
                        }
                }
            };
            request.Download(stream1);
            return filePath;
        }


        public List<GoogleDriveFile> DeleteFile([FromBody] DriveFilterBodyResponse sfileName)
        {
            string fileName = sfileName.fileName;
            Google.Apis.Drive.v3.DriveService service = GetService();

            try
            {
                // Initial validation.
                if (service == null)
                    throw new ArgumentNullException("service");

                if (string.IsNullOrEmpty(fileName))
                    throw new ArgumentNullException("fileName");

                // Get the file ID for the given file name.
                Google.Apis.Drive.v3.FilesResource.ListRequest listRequest = service.Files.List();
                listRequest.Q = $"name contains '{Path.GetFileNameWithoutExtension(fileName)}' and trashed = false";
                listRequest.Fields = "files(id, name)";
                IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;

                // If no file with the given name is found, throw an exception.
                if (files == null || files.Count == 0)
                    throw new Exception($"File with name '{fileName}' not found.");

                // Delete the file with the given ID.
                string fileId = files[0].Id;
                service.Files.Delete(fileId).Execute();
            }
            catch (Exception ex)
            {
                throw new Exception("Request Files.Delete failed.", ex);
            }

            List<GoogleDriveFile> FileList = new List<GoogleDriveFile>();
            return FileList;
        }



    }
}