using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Web.Host.Common
{
    public interface IFileDataResult
    {
        string FileType { get; set; }
        string FileUrl { get; set; }
    }
    public class FileDataResult : IFileDataResult
    {
        public string FileType { get; set; }
        public string FileUrl { get; set; }
        public FileDataResult(string fileType, string fileUrl)
        {
            this.FileType = fileType;
            this.FileUrl = fileUrl;
        }
    }
}
