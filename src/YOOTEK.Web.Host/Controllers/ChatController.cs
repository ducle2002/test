using Abp.AspNetCore.Mvc.Authorization;
using Abp.Auditing;
using Abp.IO.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using Abp.Web.Models;
using Yootek.Chat;
using Yootek.Configuration;
using Yootek.Controllers;
using Yootek.GroupChats;
using Yootek.Storage;
using Yootek.Web.Host.Chat;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Yootek.Web.Host.Controllers
{
    [DisableAuditing]
    public class ChatController : YootekControllerBase
    {
        private readonly IAbpSession _session;
        private static IHubContext<ChatHub> ChatHub;
        private readonly IGroupChatManager _groupChatManager;
        private readonly IChatMessageManager _chatMessageManager;
        private readonly IConfigurationRoot _appConfiguration;

        protected readonly IBinaryObjectManager BinaryObjectManager;

        public ChatController(
            IAbpSession session,
            IHubContext<ChatHub> chatHub,
            IGroupChatManager groupChatManager,
            IChatMessageManager chatMessageManager,
            IWebHostEnvironment env,
            IBinaryObjectManager binaryObjectManager

        )
        {
            _session = session;
            ChatHub = chatHub;
            _groupChatManager = groupChatManager;
            _chatMessageManager = chatMessageManager;
            _appConfiguration = env.GetAppConfiguration();
            BinaryObjectManager = binaryObjectManager;
        }

        [HttpPost]
        [AbpMvcAuthorize]
        public async Task<JsonResult> UploadFile()
        {
            try
            {
                var file = Request.Form.Files.First();

                //Check input
                if (file == null)
                {
                    throw new UserFriendlyException(L("File_Empty_Error"));
                }

                if (file.Length > 10000000) //10MB
                {
                    throw new UserFriendlyException(L("File_SizeLimit_Error"));
                }

                byte[] fileBytes;
                using (var stream = file.OpenReadStream())
                {
                    fileBytes = stream.GetAllBytes();
                }

                var fileObject = new BinaryObject(null, fileBytes, $"File uploaded from chat by {AbpSession.UserId}, File name: {file.FileName} {DateTime.UtcNow}");
                using (CurrentUnitOfWork.SetTenantId(null))
                {
                    await BinaryObjectManager.SaveAsync(fileObject);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }

                return Json(new AjaxResponse(new
                {
                    id = fileObject.Id,
                    name = file.FileName,
                    contentType = file.ContentType
                }));
            }
            catch (UserFriendlyException ex)
            {
                return Json(new AjaxResponse(new ErrorInfo(ex.Message)));
            }
        }

        public async Task<ActionResult> GetUploadedObject(Guid fileId, string fileName, string contentType)
        {
            using (CurrentUnitOfWork.SetTenantId(null))
            {
                var fileObject = await BinaryObjectManager.GetOrNullAsync(fileId);
                if (fileObject == null)
                {
                    return StatusCode((int)HttpStatusCode.NotFound);
                }

                return File(fileObject.Bytes, contentType, fileName);
            }
        }
    }
}
