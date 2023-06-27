using GoogleDriveMaster_API.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace GoogleDriveMaster_API.Controllers
{
    [System.Web.Http.RoutePrefix("Api/Drive")]
    public class DriveController : ApiController
    {

        [System.Web.Http.Route("GetDriveFileByName")]
        [System.Web.Http.ActionName("GetDriveFileByName")]
        [System.Web.Http.HttpGet]
        public byte[] GetDriveFileByName([FromBody] DriveFilterBodyResponse myObject)
        {
            GoogleDriveFilesRepository nServicebyname = new GoogleDriveFilesRepository(myObject);
            byte[] nResponse = nServicebyname.GetDriveFileByName(myObject);

            return nResponse;
        }


        [System.Web.Http.Route("DeleteFile")]
        [System.Web.Http.ActionName("DeleteFile")]
        [System.Web.Http.HttpPost]
        public List<GoogleDriveFile> DeleteFile([FromBody] DriveFilterBodyResponse myObject)
        {
            GoogleDriveFilesRepository nServiceDel = new GoogleDriveFilesRepository(myObject);
            List<GoogleDriveFile> nResponse = nServiceDel.DeleteFile(myObject);
            return nResponse;
           
        }


        [System.Web.Http.Route("CreateFolder")]
        [System.Web.Http.ActionName("CreateFolder")]
        [System.Web.Http.HttpPost]
        public List<GoogleDriveFile> CreateFolder([FromBody] DriveFilterBodyResponse myObject)
        {
            GoogleDriveFilesRepository nServiceCretF = new GoogleDriveFilesRepository(myObject);
            List<GoogleDriveFile> nResponse = nServiceCretF.CreateFolder(myObject);
           
            return nResponse;
        }


        [System.Web.Http.Route("UploadFile")]
        [System.Web.Http.ActionName("UploadFile")]
        [System.Web.Http.HttpPost]
        public List<GoogleDriveFile> UploadFile([FromUri]DriveFilterBodyResponse myObject)
        {
            GoogleDriveFilesRepository nServiceUPfile = new GoogleDriveFilesRepository(myObject);
            List<GoogleDriveFile> nResponse = nServiceUPfile.FileUpload(myObject);
            return nResponse;
        }


        [System.Web.Http.Route("FileUploadInFolder")]
        [System.Web.Http.ActionName("FileUploadInFolder")]
        [System.Web.Http.HttpPost]
        public List<GoogleDriveFile> FileUploadInFolder([FromUri]DriveFilterBodyResponse myObject)
        {
            GoogleDriveFilesRepository nServiceUPinFol = new GoogleDriveFilesRepository(myObject);
            List<GoogleDriveFile> nResponse = nServiceUPinFol.FileUploadInFolder(myObject.FolderName);
            return nResponse;
        }


        [System.Web.Http.Route("DownloadFile")]
        [System.Web.Http.ActionName("DownloadFile")]
        [System.Web.Http.HttpGet]
        public void DownloadFile([FromBody] DriveFilterBodyResponse myObject)
        {
            string FilePath = new GoogleDriveFilesRepository(myObject).DownloadGoogleFile(myObject.fileName);

            HttpContext.Current.Response.ContentType = "application/zip";
            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(FilePath));
            HttpContext.Current.Response.WriteFile(System.Web.HttpContext.Current.Server.MapPath("~/GoogleDriveFiles/" + Path.GetFileName(FilePath)));
            HttpContext.Current.Response.End();
            HttpContext.Current.Response.Flush();
        }

    }
}
