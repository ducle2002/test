using Abp;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Configuration;
using Abp.Notifications;
using Abp.Runtime.Session;
using Abp.UI;
using Yootek.Notifications.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Yootek.Lib.CrudBase;
using Microsoft.Extensions.Configuration;
using Yootek.Extensions;
using Yootek.Common.DataResult;

namespace Yootek.Notifications
{
    [AbpAuthorize]
    public class NotificationAppService : YootekAppServiceBase, INotificationAppService
    {
        private readonly INotificationDefinitionManager _notificationDefinitionManager;
        private readonly IUserNotificationManager _userNotificationManager;
        private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
        private readonly IAppNotifier _appNotifier;
        private readonly HttpClient _httpClient;

        public NotificationAppService(
            INotificationDefinitionManager notificationDefinitionManager,
            IUserNotificationManager userNotificationManager,
            IAppNotifier appNotifier,
            INotificationSubscriptionManager notificationSubscriptionManager,
            YootekHttpClient yootekHttpClient,
            IConfiguration configuration)
        {
            _notificationDefinitionManager = notificationDefinitionManager;
            _userNotificationManager = userNotificationManager;
            _notificationSubscriptionManager = notificationSubscriptionManager;
            _appNotifier = appNotifier;
            _httpClient = yootekHttpClient.GetHttpClient(configuration["ApiSettings:Yootek.Notification"]);
        }

        [DisableAuditing]
        public async Task<GetNotificationsOutputOld> GetUserNotifications(GetUserNotificationsInput input)
        {
            try
            {
                //var result =  await _httpClient.SendAsync<GetNotificationsOutput>("/api/services/app/Notification/GetUserNotifications", HttpMethod.Get, input);

                //var totalCount = await _userNotificationManager.GetUserNotificationCountAsync(
                //  AbpSession.ToUserIdentifier(), input.State, input.StartDate, input.EndDate
                //  );

                //var unreadCount = await _userNotificationManager.GetUserNotificationCountAsync(
                //    AbpSession.ToUserIdentifier(), UserNotificationState.Unread, input.StartDate, input.EndDate
                //    );
                //var notifications = await _userNotificationManager.GetUserNotificationsAsync(
                //    AbpSession.ToUserIdentifier(), input.State, input.SkipCount, input.MaxResultCount, input.StartDate, input.EndDate
                //    );
                // return new GetNotificationsOutputOld(result.Data.TotalRecords, result.Data.UnreadCount, result.Data.Data);
                throw new Exception();
            }
            catch( Exception ex )
            {
                throw;
            }
        }

        public async Task SetAllNotificationsAsRead()
        {
            await _userNotificationManager.UpdateAllUserNotificationStatesAsync(AbpSession.ToUserIdentifier(), UserNotificationState.Read);
        }

        public async Task SetNotificationAsRead(EntityDto<Guid> input)
        {
            var userNotification = await _userNotificationManager.GetUserNotificationAsync(AbpSession.TenantId, input.Id);
            if (userNotification.UserId != AbpSession.GetUserId())
            {
                return;
            }

            if (userNotification.UserId != AbpSession.GetUserId())
            {
                throw new Exception(string.Format("Given user notification id ({0}) is not belong to the current user ({1})", input.Id, AbpSession.GetUserId()));
            }

            await _userNotificationManager.UpdateUserNotificationStateAsync(AbpSession.TenantId, input.Id, UserNotificationState.Read);
        }

        public async Task<GetNotificationSettingsOutput> GetNotificationSettings()
        {
            var output = new GetNotificationSettingsOutput();

            output.ReceiveNotifications = await SettingManager.GetSettingValueAsync<bool>(NotificationSettingNames.ReceiveNotifications);

            output.Notifications = (await _notificationDefinitionManager
                .GetAllAvailableAsync(AbpSession.ToUserIdentifier()))
                .Where(nd => nd.EntityType == null) //Get general notifications, not entity related notifications.
                .MapTo<List<NotificationSubscriptionWithDisplayNameDto>>();

            var subscribedNotifications = (await _notificationSubscriptionManager
                .GetSubscribedNotificationsAsync(AbpSession.ToUserIdentifier()))
                .Select(ns => ns.NotificationName)
                .ToList();

            output.Notifications.ForEach(n => n.IsSubscribed = subscribedNotifications.Contains(n.Name));

            return output;
        }

        public async Task UpdateNotificationSettings(UpdateNotificationSettingsInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), NotificationSettingNames.ReceiveNotifications, input.ReceiveNotifications.ToString());

            foreach (var notification in input.Notifications)
            {
                if (notification.IsSubscribed)
                {
                    await _notificationSubscriptionManager.SubscribeAsync(AbpSession.ToUserIdentifier(), notification.Name);
                }
                else
                {
                    await _notificationSubscriptionManager.UnsubscribeAsync(AbpSession.ToUserIdentifier(), notification.Name);
                }
            }
        }

        public async Task DeleteNotification(EntityDto<Guid> input)
        {
            var notification = await _userNotificationManager.GetUserNotificationAsync(AbpSession.TenantId, input.Id);
            if (notification == null)
            {
                return;
            }

            if (notification.UserId != AbpSession.GetUserId())
            {
                throw new UserFriendlyException(L("ThisNotificationDoesntBelongToYou"));
            }

            await _userNotificationManager.DeleteUserNotificationAsync(AbpSession.TenantId, input.Id);
        }

        public async Task DeleteAllUserNotifications(DeleteAllUserNotificationsInput input)
        {
            await _userNotificationManager.DeleteAllUserNotificationsAsync(
                AbpSession.ToUserIdentifier(),
                input.State,
                input.StartDate,
                input.EndDate);
        }
    }
}