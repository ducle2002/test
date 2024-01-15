using Abp;
using Abp.Auditing;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.IO.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using Abp.Web.Models;
using Yootek.Authorization.Users;
using Yootek.Controllers;
using Yootek.Friendships;
using Yootek.Net.MimeTypes;
using Yootek.Storage;
using Yootek.Web.Host.Controllers.ModelView;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Web.Host.Controllers
{
    [DisableAuditing]
    public class ProfileController : YootekControllerBase
    {

        private readonly UserManager _userManager;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IAppFolders _appFolders;
        private readonly IFriendshipManager _friendshipManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ITempFileCacheManager _tempFileCacheManager;

        private const int MaxProfilePictureSize = 5242880; //5MB

        public ProfileController(
            UserManager userManager,
            IBinaryObjectManager binaryObjectManager,
            IAppFolders appFolders,
            IFriendshipManager friendshipManager,
            ITempFileCacheManager tempFileCacheManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _binaryObjectManager = binaryObjectManager;
            _appFolders = appFolders;
            _friendshipManager = friendshipManager;
            _webHostEnvironment = webHostEnvironment;
            _tempFileCacheManager = tempFileCacheManager;
        }

        [HttpGet]
        [Route("getProfilePicture")]
        [DisableAuditing]
        public async Task<FileResult> GetProfilePicture()
        {
            var user = await _userManager.GetUserByIdAsync(AbpSession.GetUserId());
            if (user.ProfilePictureId == null)
            {
                return GetDefaultProfilePicture();
            }

            return await GetProfilePictureById(user.ProfilePictureId.Value);
        }

        [HttpGet]
        [Route("getProfilePictureById/{id}")]
        [DisableAuditing]
        public async Task<FileResult> GetProfilePictureById(string id = "")
        {
            if (id.IsNullOrEmpty())
            {
                return GetDefaultProfilePicture();
            }

            return await GetProfilePictureById(Guid.Parse(id));
        }

        [DisableAuditing]
        [UnitOfWork]
        public virtual async Task<FileResult> GetFriendProfilePictureById(long userId, int? tenantId, string id = "")
        {
            if (id.IsNullOrEmpty() ||
              await _friendshipManager.GetFriendshipOrNullAsync(AbpSession.ToUserIdentifier(), new UserIdentifier(tenantId, userId)) == null)
            {
                return GetDefaultProfilePicture();
            }

            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                return await GetProfilePictureById(Guid.Parse(id));
            }
        }

        [HttpPost]
        [Route("changeProfilePicture")]
        [UnitOfWork]
        public virtual async Task<JsonResult> ChangeProfilePicture()
        {
            try
            {
                var request = await Request.ReadFormAsync();
                //Check input
                if (request.Files.Count <= 0 || request.Files[0] == null)
                {
                    throw new UserFriendlyException(L("ProfilePicture_Change_Error"));
                }

                var file = request.Files[0];

                if (file.Length > 30720) //30KB.
                {
                    throw new UserFriendlyException(L("ProfilePicture_Warn_SizeLimit"));
                }

                //Get user
                var user = await _userManager.GetUserByIdAsync(AbpSession.GetUserId());

                //Delete old picture
                if (user.ProfilePictureId.HasValue)
                {
                    await _binaryObjectManager.DeleteAsync(user.ProfilePictureId.Value);
                }

                //Save new picture
                var storedFile = new BinaryObject(AbpSession.TenantId, ConvertToBytes(file));
                await _binaryObjectManager.SaveAsync(storedFile);

                //Update new picture on the user
                user.ProfilePictureId = storedFile.Id;

                //Return success
                return Json(new AjaxResponse());
            }
            catch (UserFriendlyException ex)
            {
                //Return error message
                return Json(new AjaxResponse(new ErrorInfo(ex.Message)));
            }
        }

        [HttpPost]
        [Route("UploadProfilePicture")]
        public UploadProfilePictureOutput UploadProfilePicture(FileDto input)
        {
            try
            {
                var profilePictureFile = Request.Form.Files.First();

                //Check input
                if (profilePictureFile == null)
                {
                    throw new UserFriendlyException(L("ProfilePicture_Change_Error"));
                }

                if (profilePictureFile.Length > MaxProfilePictureSize)
                {
                    throw new UserFriendlyException(L("ProfilePicture_Warn_SizeLimit", AppConsts.MaxProfilPictureBytesUserFriendlyValue));
                }

                byte[] fileBytes;
                using (var stream = profilePictureFile.OpenReadStream())
                {
                    fileBytes = stream.GetAllBytes();
                }

                if (!ImageFormatHelper.GetRawImageFormat(fileBytes).IsIn(ImageFormat.Jpeg, ImageFormat.Png, ImageFormat.Gif))
                {
                    throw new Exception(L("IncorrectImageFormat"));
                }

                _tempFileCacheManager.SetFile(input.FileToken, fileBytes);

                using (var bmpImage = new Bitmap(new MemoryStream(fileBytes)))
                {
                    return new UploadProfilePictureOutput
                    {
                        FileToken = input.FileToken,
                        FileName = input.FileName,
                        FileType = input.FileType,
                        Width = bmpImage.Width,
                        Height = bmpImage.Height
                    };
                }
            }
            catch (UserFriendlyException ex)
            {
                return new UploadProfilePictureOutput(new ErrorInfo(ex.Message));
            }
        }

        private FileResult GetDefaultProfilePicture()
        {
            string webRootPath = _webHostEnvironment.WebRootPath;

            var path = Path.Combine(webRootPath + "/Common/Images/default-profile-picture.png");
            return File(System.IO.File.ReadAllBytes(path), MimeTypeNames.ImagePng);
        }

        private async Task<FileResult> GetProfilePictureById(Guid profilePictureId)
        {
            var file = await _binaryObjectManager.GetOrNullAsync(profilePictureId);
            if (file == null)
            {
                return GetDefaultProfilePicture();
            }

            return File(file.Bytes, MimeTypeNames.ImageJpeg);
        }

        private byte[] ConvertToBytes(IFormFile image)
        {
            using (var memoryStream = new MemoryStream())
            {
                image.OpenReadStream().CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }


    }

    public class ImageFormatHelper
    {
        public static ImageFormat GetRawImageFormat(byte[] fileBytes)
        {
            using (var ms = new MemoryStream(fileBytes))
            {
                var fileImage = Image.FromStream(ms);
                return fileImage.RawFormat;
            }
        }
    }
}
