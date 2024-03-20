using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Yootek.App.ServiceHttpClient.Dto;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity.WorkDtos;
using Yootek.App.ServiceHttpClient.Yootek.SmartCommunity;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Notifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Yootek.Common.Enum.UserFeedbackEnum;

namespace Yootek.Services
{
    public interface IWorkNotifyAppService
    {
        Task SchedulerWorkCreateNotificationAsync();
    }
    public class WorkNotifyAppService : YootekAppServiceBase, IWorkNotifyAppService
    {
        private readonly IHttpWorkAssignmentService _httpWorkAssignmentService;
        private readonly IAppNotifier _appNotifier;
        public WorkNotifyAppService(
            IHttpWorkAssignmentService httpWorkAssignmentService,
             UserManager userManager,
             IAppNotifier appNotifier)
        {
            _httpWorkAssignmentService = httpWorkAssignmentService;
            _appNotifier = appNotifier;
        }

        [AbpAllowAnonymous]
        [RemoteService(true)]
        public async Task SchedulerWorkCreateNotificationAsync()
        {
            MicroserviceResultDto<PagedResultDto<GetAllWorksNotifyDto>> result = await _httpWorkAssignmentService.GetListWorkNotify(new()
            {
                QueryCase = QueryCaseWorkNotify.ByWorker
            });
            if (result.Success)
            {
                string detailUrlApp = "yoolife://app/work";
                foreach (var item in result.Result.Items)
                {
                    if (item.UserIds != null && item.UserIds.Count() > 0)
                    {
                        var messageAcceptFeed = new UserMessageNotificationDataBase(
                         AppNotificationAction.WorkNotification,
                        AppNotificationIcon.WorkNotificationIcon,
                        TypeAction.Detail,
                        item.Title,
                        detailUrlApp,
                        "",
                        "",
                        "",
                        item.Id
                    );
                        await _appNotifier.SendUserMessageNotifyFullyAsync(
                            "Thông báo công việc cần thực hiện",
                            "Bạn có công việc cần thực hiện",
                            detailUrlApp,
                            "",
                            item.UserIds.Select(x => { return new UserIdentifier(item.TenantId, x); }).ToArray(),
                            messageAcceptFeed);
                    }
                    
                }
            }
        }
    }
}
