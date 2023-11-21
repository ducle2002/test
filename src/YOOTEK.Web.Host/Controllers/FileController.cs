using Abp;
using Abp.Auditing;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Abp.UI;
using DocumentFormat.OpenXml.Wordprocessing;
using IMAX.Chat;
using IMAX.Common.DataResult;
using IMAX.Configuration;
using IMAX.Controllers;
using IMAX.Core.Dto;
using IMAX.GroupChats;
using IMAX.Storage;
using IMAX.Web.Host.Chat;
using IMAX.Web.Host.Common;
using IMAX.Web.Host.ModelView;
using ImaxFileUploaderServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace IMAX.Web.Host.Controllers
{
    public class FileController : IMAXControllerBase
    {
        private readonly IAbpSession _session;
        private static IHubContext<ChatHub> ChatHub;
        private readonly IGroupChatManager _groupChatManager;
        private readonly IChatMessageManager _chatMessageManager;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly ITempFileCacheManager _tempFileCacheManager;
        private readonly IS3Service _s3Service;

        public FileController(
            IAbpSession session,
            IHubContext<ChatHub> chatHub,
            IGroupChatManager groupChatManager,
            IChatMessageManager chatMessageManager,
            IWebHostEnvironment env,
            ITempFileCacheManager tempFileCacheManager,
            IS3Service s3Service
        )
        {
            _session = session;
            ChatHub = chatHub;
            _groupChatManager = groupChatManager;
            _chatMessageManager = chatMessageManager;
            _tempFileCacheManager = tempFileCacheManager;
            _appConfiguration = env.GetAppConfiguration();
            _s3Service = s3Service;
        }

        [DisableAuditing]
        [HttpGet]
        [Route("DownloadTempFile")]
        public ActionResult DownloadTempFile(FileDto file)
        {
            var fileBytes = _tempFileCacheManager.GetFile(file.FileToken);
            if (fileBytes == null)
            {
                return NotFound(L("RequestedFileDoesNotExists"));
            }

            return File(fileBytes, file.FileType, file.FileName);
        }


        [HttpGet]
        [Route("downfile/{filePath}")]
        public IActionResult DownloadFile(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound();
                }

                var FileType = "";
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, FileType, filePath);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }

        [HttpPost]
        [Route("uploadimagepublic")]
        public async Task<string> UploadImagePublic()
        {
            try
            {
                var request = await Request.ReadFormAsync();
                //Check input

                var ufile = request.Files[0];
                if (ufile != null && ufile.Length > 0)
                {
                    //var clientUp = new HttpClient();
                    //var formData = new MultipartFormDataContent();

                    //// Add the file content to the form data

                    //using (var streamContent = new StreamContent(ufile.OpenReadStream()))
                    //{
                    //    // Add the file as stream content to the multipart form data content
                    //    formData.Add(streamContent, "file", ufile.FileName);

                    //    // Send the request with the multipart form data content
                    //    var response = await clientUp.PostAsync(_appConfiguration["ApiSettings:FileUploadS3"] + "/api/FileUpload/UploadFile", formData);

                    //    var data = await response.Content.ReadAsStringAsync();

                    //    try
                    //    {
                    //        var jsondata = JsonConvert.DeserializeObject<dynamic>(data);
                    //        return (string)jsondata.data;
                    //    }
                    //    catch (Exception)
                    //    {

                    //    }
                    //}

                    var fileExt = Path.GetExtension(ufile.FileName);

                    var keyName =
                        $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{new Random().Next(1000, 9999)}{fileExt}";


                    var url = await _s3Service.UploadToPublic(keyName, ufile);

                    return url;

                }

                throw new UserFriendlyException("Invalid input formdata !");
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"{DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy")} CheckFilePdfValid {ex.Message} {JsonConvert.SerializeObject(ex)}");
                throw new UserFriendlyException(ex.Message);
            }
        }

        [HttpPost]
        [Route("uploadlistimagepublic")]
        public async Task<object> UploadListImage()
        {
            try
            {
                var request = await Request.ReadFormAsync();
                //Check input

                var ufiles = request.Files;

                var clientUp = new HttpClient();

                if (ufiles != null && ufiles.Count > 0)
                {
                    var result = new List<string>();
                    foreach (var ufile in ufiles)
                    {


                        //using (var streamContent = new StreamContent(ufile.OpenReadStream()))
                        //{
                        //    var formData = new MultipartFormDataContent();
                        //    // Add the file as stream content to the multipart form data content
                        //    formData.Add(streamContent, "file", ufile.FileName);

                        //    // Send the request with the multipart form data content
                        //    var response = await clientUp.PostAsync(_appConfiguration["ApiSettings:FileUploadS3"] + "/api/FileUpload/UploadFile", formData);

                        //    var data = await response.Content.ReadAsStringAsync();

                        //    try
                        //    {
                        //        var jsondata = JsonConvert.DeserializeObject<dynamic>(data);
                        //        var url = (string)jsondata.data;
                        //        result.Add(url);
                        //    }
                        //    catch (Exception e)
                        //    {

                        //    }
                        //}
                        var fileExt = Path.GetExtension(ufile.FileName);

                        var keyName =
                            $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{new Random().Next(1000, 9999)}{fileExt}";

                        var url = await _s3Service.UploadToPublic(keyName, ufile);
                        result.Add(url);

                    }

                    return result;
                }

                throw new UserFriendlyException("Invalid input formdata !");
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"{DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy")} CheckFilePdfValid {ex.Message} {JsonConvert.SerializeObject(ex)}");
                throw new UserFriendlyException(ex.Message);
            }
        }

        [HttpPost]
        [Route("uploadfile")]
        public async Task<object> UploadFile()
        {
            try
            {
                var request = await Request.ReadFormAsync();
                //Check input

                var ufile = request.Files[0];
                if (ufile != null && ufile.Length > 0)
                {
                    var fileExt = Path.GetExtension(ufile.FileName);
                    // generate base on "timestamp" + random number
                    var keyName =
                        $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{new Random().Next(1000, 9999)}{fileExt}";


                    var url = await _s3Service.UploadToPublic(keyName, ufile);

                    return url;

                }
               
                throw new UserFriendlyException("Invalid input formdata !");
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }
        [HttpPost]
        [Route("upload-list-file")]
        public async Task<object> UploadListFile()
        {
            try
            {
                var request = await Request.ReadFormAsync();
                //Check input

                var ufiles = request.Files;

                var clientUp = new HttpClient();

                if (ufiles != null && ufiles.Count > 0)
                {
                    var result = new List<string>();
                    foreach (var ufile in ufiles)
                    {
                 
                        using (var streamContent = new StreamContent(ufile.OpenReadStream()))
                        {
                            var formData = new MultipartFormDataContent();
                            // Add the file as stream content to the multipart form data content
                            formData.Add(streamContent, "file", ufile.FileName);

                            // Send the request with the multipart form data content
                            var response = await clientUp.PostAsync(_appConfiguration["ApiSettings:FileUploadS3"] + "/api/FileUpload/UploadFile", formData);

                            var data = await response.Content.ReadAsStringAsync();

                            try
                            {
                                var jsondata = JsonConvert.DeserializeObject<dynamic>(data);
                                var url = (string)jsondata.data;
                                result.Add(url);
                            }
                            catch (Exception e)
                            {

                            }
                        }

                    }

                    return result;
                }


                throw new UserFriendlyException("Invalid input formdata !");
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"{DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy")} CheckFilePdfValid {ex.Message} {JsonConvert.SerializeObject(ex)}");
                throw new UserFriendlyException(ex.Message);
            }
        }


        [HttpPost("UploadBatchImage")]
        public async Task<object> UploadBatchImage(IEnumerable<IFormFile> files, string path)
        {
            var maxFileSize = 1024 * 1024 * 5; // 5MB
            foreach (var file in files)
            {
                var validImageTypes = new[] { "image/gif", "image/jpeg", "image/pjpeg", "image/png" };
                if (!validImageTypes.Contains(file.ContentType))
                {
                    return new { success = false, message = "Invalid file type." };
                }

                if (file.Length > maxFileSize)
                {
                    return new { success = false, message = "File size is too large." };
                }
            }

            var result = new List<string>();
            foreach (var file in files)
            {
                var fileExt = Path.GetExtension(file.FileName);

                if (!string.IsNullOrEmpty(path) && !path.EndsWith("/"))
                {
                    path = $"{path}/";
                }

                // generate base on "timestamp" + random number
                var keyName =
                    $"{path}{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{new Random().Next(1000, 9999)}{fileExt}";


                var url = await _s3Service.UploadToPublic(keyName, file);
                result.Add(url);
            }

            return new { success = true, data = result };
        }

        [HttpPost("UploadOneImage")]
        public async Task<object> UploadImage(IFormFile file, string path)
        {
            var maxFileSize = 1024 * 1024 * 5; // 5MB
            var validImageTypes = new[] { "image/gif", "image/jpeg", "image/pjpeg", "image/png" };
            if (!validImageTypes.Contains(file.ContentType))
            {
                return new { success = false, message = "Invalid file type." };
            }

            if (file.Length > maxFileSize)
            {
                return new { success = false, message = "File size is too large." };
            }

            var fileExt = Path.GetExtension(file.FileName);

            if (!string.IsNullOrEmpty(path) && !path.EndsWith("/"))
            {
                path = $"{path}/";
            }

            // generate base on "timestamp" + random number
            var keyName =
                $"{path}{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{new Random().Next(1000, 9999)}{fileExt}";


            var url = await _s3Service.UploadToPublic(keyName, file);

            return new { success = true, data = url };
        }

        [HttpPost("UploadBatchFile")]
        public async Task<object> UploadBatchFile(IEnumerable<IFormFile> files, string path)
        {
            var result = new List<string>();
            foreach (var file in files)
            {
                var fileExt = Path.GetExtension(file.FileName);

                if (!string.IsNullOrEmpty(path) && !path.EndsWith("/"))
                {
                    path = $"{path}/";
                }

                // generate base on "timestamp" + random number
                var keyName =
                    $"{path}{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{new Random().Next(1000, 9999)}{fileExt}";

                var url = await _s3Service.UploadToPublic(keyName, file);
                result.Add(url);
            }

            return new { success = true, data = result };
        }

        [HttpPost("UploadOneFile")]
        public async Task<object> UploadFile(IFormFile file, string path)
        {
            var fileExt = Path.GetExtension(file.FileName);

            if (!string.IsNullOrEmpty(path) && !path.EndsWith("/"))
            {
                path = $"{path}/";
            }

            // generate base on "timestamp" + random number
            var keyName =
                $"{path}{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{new Random().Next(1000, 9999)}{fileExt}";


            var url = await _s3Service.UploadToPublic(keyName, file);

            return new { success = true, data = url };
        }

        [HttpPost("UploadExcelFile")]
        public async Task<object> UploadExcelFile(IFormFile file, string path)
        {
            var fileExt = Path.GetExtension(file.FileName);
            var validFileTypes = new[] { ".xls", ".xlsx" };
            if (!validFileTypes.Contains(fileExt))
            {
                return new { success = false, message = "Invalid file type." };
            }

            if (!string.IsNullOrEmpty(path) && !path.EndsWith("/"))
            {
                path = $"{path}/";
            }

            // generate base on "timestamp" + random number
            var keyName =
                $"{path}{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{new Random().Next(1000, 9999)}{fileExt}";


            var url = await _s3Service.UploadToPublic(keyName, file);

            return new { success = true, data = url };
        }
  

        private int CheckFilePdfValid(IFormFile input, int sizeLimit, string[] typeExcepted)
        {
            try
            {
                var fileExtension = Path.GetExtension(input.FileName).ToLower();
                if (input.Length > sizeLimit)
                {
                    return 1;
                }

                if (!typeExcepted.Any(fileExtension.Contains))
                {
                    return 2;
                }

                return 3;
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"{DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy")} CheckFilePdfValid {ex.Message} {JsonConvert.SerializeObject(ex)}");
                return 0;
            }
        }
    }
}