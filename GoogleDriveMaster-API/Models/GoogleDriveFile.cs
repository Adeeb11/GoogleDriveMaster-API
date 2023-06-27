using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoogleDriveMaster_API.Models
{
    public class GoogleDriveFile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long? Size { get; set; }
        public long? Version { get; set; }
        public DateTime? CreatedTime { get; set; }
        public IList<string> Parents { get; set; }
        public string MimeType { get; set; }
    }

    public class DriveFilterBodyResponse
    {
        public string ClientCode { get; set; }
        public string FolderName { get; set; }
        public string fileName { get; set; }
      


    }
}