using Abp;
using Abp.BackgroundJobs;
using Abp.Collections.Extensions;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Json;
using Abp.Localization;
using Abp.Notifications;
using Abp.RealTime;
using Abp.Runtime.Session;
using IMAX.Authorization.Users;
using IMAX.EntityDb;
using IMAX.MultiTenancy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMAX.Notifications
{
    public class AppNotifier : IMAXDomainServiceBase, IAppNotifier
    {
        public const int MaxUserCountToDirectlyDistributeANotification = 100;

        /// <summary>
        /// Indicates all tenants.
        /// </summary>
        public static int[] AllTenants => new[] { NotificationInfo.AllTenantIds.To<int>() };

        /// <summary>
        /// Reference to ABP session.
        /// </summary>
        public IAbpSession AbpSession { get; set; }

        private readonly INotificationStore _store;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly INotificationDistributer _notificationDistributer;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly INotificationCommunicator _notificationCommunicator;
        private readonly ICloudMessagingManager _cloudMessagingManager;
        private readonly IRepository<FcmTokens, long> _fcmTokenRepos;
        private readonly IRepository<NotificationInfo, Guid> _notificationInfoRepos;

        public AppNotifier(
            INotificationPublisher notificationPublisher,
            INotificationCommunicator notificationCommunicator,
            ICloudMessagingManager cloudMessagingManager,
            INotificationStore store,
            IBackgroundJobManager backgroundJobManager,
            INotificationDistributer notificationDistributer,
            IGuidGenerator guidGenerator,
            IRepository<FcmTokens, long> fcmTokenRepos,
            IOnlineClientManager onlineClientManager
        )
        {
            _notificationPublisher = notificationPublisher;
            _notificationCommunicator = notificationCommunicator;
            _cloudMessagingManager = cloudMessagingManager;
            _store = store;
            _backgroundJobManager = backgroundJobManager;
            _notificationDistributer = notificationDistributer;
            _guidGenerator = guidGenerator;
            AbpSession = NullAbpSession.Instance;
            _fcmTokenRepos = fcmTokenRepos;
            _onlineClientManager = onlineClientManager;
        }


        public async Task NewTenantRegisteredAsync(Tenant tenant)
        {
            var notificationData = new LocalizableMessageNotificationData(
                new LocalizableString(
                    "NewTenantRegisteredNotificationMessage",
                    IMAXConsts.LocalizationSourceName
                )
            );

            notificationData["tenancyName"] = tenant.TenancyName;
            await PublishAsync(AppNotificationNames.NewTenantRegistered, notificationData);
        }

        // Chỉ gửi thông báo lưu
        /// <summary>
        /// This function sends a notification to the specified users and also sends a firebase
        /// notification to the specified users
        /// </summary>
        /// <param name="notificationName">The name of the notification.</param>
        /// <param name="fireBaseMessage">The message that will be sent to the user's device.</param>
        /// <param name="users">The users to send the notification to.</param>
        /// <param name="NotificationData">This is the data that will be sent to the client.</param>
        /// <param name="NotificationSeverity">This is the type of notification. It can be Info,
        /// Success, Warn, Error.</param>
        public async Task SendUserMessageOnlySaveAsync(string notificationName,  UserIdentifier[] users, UserMessageNotificationDataBase messageNotification, NotificationSeverity severity = NotificationSeverity.Info)
        {
            await PublishAsync(
                notificationName,
                messageNotification,
                severity: severity,
                userIds: users
            );

        }

        // Chỉ gửi thông báo qua firebase
        /// <summary>
        /// This function sends a notification to the specified users and also sends a firebase
        /// notification to the specified users
        /// </summary>
        /// <param name="notificationName">The name of the notification.</param>
        /// <param name="fireBaseMessage">The message that will be sent to the user's device.</param>
        /// <param name="users">The users to send the notification to.</param>
        /// <param name="NotificationData">This is the data that will be sent to the client.</param>
        /// <param name="NotificationSeverity">This is the type of notification. It can be Info,
        /// Success, Warn, Error.</param>
        public async Task SendUserMessageNotifyFireBaseAsync(string notificationName, string fireBaseMessage, string detailUrlApp, string detailUrlWA, UserIdentifier[] users, UserMessageNotificationDataBase messageNotification, NotificationSeverity severity = NotificationSeverity.Info)
        {
            await PublishAsync(
                notificationName,
                messageNotification,
                severity: severity,

                userIds: users

            );
            if (users.Length > 0)
            {
                var userIds = users.Select(x => x.UserId).ToList();
                await SendMessageFireBaseNotify(fireBaseMessage, notificationName, messageNotification.Action, userIds, detailUrlApp, detailUrlWA);
            }
        }

        //Gửi thông báo Firebase nếu offline, realtime nếu online
        /// <summary>
        /// Send a notification to a list of users, if the user is online, send the notification realtime to the
        /// user, if the user is offline, send a notification to the user via Firebase
        /// </summary>
        /// <param name="notificationName">The name of the notification.</param>
        /// <param name="fireBaseMessage">The message that will be sent to the user's device.</param>
        /// <param name="users">The users to send the notification to.</param>
        /// <param name="NotificationData">This is the data that will be sent to the client.</param>
        /// <param name="NotificationSeverity">This is the type of notification, such as info, success,
        /// warning, error.</param>
        public async Task SendUserMessageNotifyFireBaseCheckOnlineAsync(string notificationName, string fireBaseMessage, string detailUrlApp, string detailUrlWA, UserIdentifier[] users, UserMessageNotificationDataBase messageNotification, NotificationSeverity severity = NotificationSeverity.Info)
        {
            await PublishAsync(
               notificationName,
               messageNotification,
               severity: severity,
               userIds: users
            );
            var userOfflines = new List<UserIdentifier>();
            var userOnlines = new List<UserIdentifier>();
            foreach (var us in users)
            {
                if (await _onlineClientManager.IsOnlineAsync(us))
                {
                    userOnlines.Add(us);
                }
                else
                {
                    userOfflines.Add(us);
                }
            }

            if (userOfflines.Count > 0)
            {
                var userIds = userOfflines.Select(x => x.UserId).ToList();
                await SendMessageFireBaseNotify(fireBaseMessage, notificationName, messageNotification.Action, userIds, detailUrlApp, detailUrlWA);
            }

            //if (userOnlines.Count > 0)
            //{
            //    await _notificationCommunicator.SendNotificationsAsync(notificationName,
            //      messageNotification, userOnlines.ToArray());
            //}

        }


        //Gửi thông báo chỉ qua Firebase
        public async Task SendUserMessageNotifyOnlyFirebaseAsync(string notificationName, string fireBaseMessage, UserIdentifier[] users, UserMessageNotificationDataBase messageNotification, string detailUrlApp, string detailUrlWA)
        {

            var userIds = users.Select(x => x.UserId).ToList();
            await SendMessageFireBaseNotify(fireBaseMessage, notificationName, messageNotification.Action, userIds, detailUrlApp, detailUrlWA);

        }

        // Chỉ gửi thông báo cả firebase và realtime
        /// <summary>
        /// Send a notification to a list of users, if they are online, send them a notification realtime,
        /// send all them a notification via Firebase
        /// </summary>
        /// <param name="notificationName">The name of the notification.</param>
        /// <param name="fireBaseMessage">The message that will be sent to the user's device.</param>
        /// <param name="users">The users to send the notification to.</param>
        /// <param name="NotificationData">This is the data that will be sent to the client.</param>
        /// <param name="NotificationSeverity">This is the type of notification.</param>
        public async Task SendUserMessageNotifyFullyAsync(string notificationName, string fireBaseMessage, string detailUrlApp, string detailUrlWA, UserIdentifier[] users, UserMessageNotificationDataBase messageNotification, int appType = 0, NotificationSeverity severity = NotificationSeverity.Info)
        {
            try
            {
                //var userOnlines = new List<UserIdentifier>();
                //foreach (var us in users)
                //{
                //    if (_onlineClientManager.IsOnline(us))
                //    {
                //        userOnlines.Add(us);
                //    }

                //}

                if (users.Length > 0)
                {
                    var userIds = users.Select(x => x.UserId).ToList();
                    await SendMessageFireBaseNotify(fireBaseMessage, notificationName, messageNotification.Action, userIds, detailUrlApp, detailUrlWA, appType);
                }

                //if (userOnlines.Count > 0)
                //{
                //    _notificationCommunicator.SendNotificationsAsync(notificationName,
                //      messageNotification, userOnlines.ToArray());
                //}
                await PublishAsync(
                   notificationName,
                   messageNotification,
                   severity: severity,
                   userIds: users
                   );

            }
            catch (Exception e)
            {

            }
        }

        public async Task MultiSendMessageAsync(string typeMessage, UserIdentifier[] users, string message,
            bool isRealtimeMessage = false, bool isCloudMessage = false, bool isSendAll = false,
            NotificationSeverity severity = NotificationSeverity.Info)
        {
            await PublishAsync(
                 typeMessage,
                 new MessageNotificationData(message),
                 severity: severity,
                 userIds: users
             );


        }

        protected async Task SendNotificationAsync(string notificationName, UserIdentifier user,
            LocalizableString localizableMessage, IDictionary<string, object> localizableMessageData = null,
            NotificationSeverity severity = NotificationSeverity.Info)
        {
            var notificationData = new LocalizableMessageNotificationData(localizableMessage);
            if (localizableMessageData != null)
            {
                foreach (var pair in localizableMessageData)
                {
                    notificationData[pair.Key] = pair.Value;
                }
            }

            await PublishAsync(notificationName, notificationData, severity: severity,
                userIds: new[] { user });
        }

        public Task SomeUsersCouldntBeImported(UserIdentifier user, string fileToken, string fileType, string fileName)
        {
            return SendNotificationAsync(AppNotificationNames.DownloadInvalidImportUsers, user,
                new LocalizableString(
                    "ClickToSeeInvalidUsers",
                    IMAXConsts.LocalizationSourceName
                ),
                new Dictionary<string, object>
                {
                    { "fileToken", fileToken },
                    { "fileType", fileType },
                    { "fileName", fileName }
                });
        }


        public async Task SendCommentPostMessageNotifyAsync(Post post, PostComment comment, User userReact)
        {
            try
            {
                var user = new UserIdentifier(post.TenantId, post.CreatorUserId ?? 0);
                string detailUrlApp = $"yoolife://forum/comment?postId={post.Id}";
                string detailUrlWA = $"/news?id={post.Id}";

                var mess = $"{userReact.UserName} đã bình luận về bài viết của bạn.";

                if (!await _onlineClientManager.IsOnlineAsync(user))
                {
                    await SendMessageFireBaseNotify(mess, "", AppNotificationAction.PostCommentAction, new List<long>() { user.UserId }, detailUrlApp, detailUrlWA);
                }

                var messageData = new UserPostMessageNotificationData(
                    AppNotificationAction.PostCommentAction,
                    AppNotificationIcon.PostCommentIcon,
                    post.Id,
                    comment.Id,
                    null,
                    mess,
                    detailUrlApp,
                    detailUrlWA,
                    userReact.ImageUrl,
                    ""
                    
                );

                await PublishAsync(
                 mess,
                 messageData,
                 severity: NotificationSeverity.Info,
                 userIds: new UserIdentifier[] { user }
                 );

            }
            catch (Exception e)
            {
            }
        }
        public async Task SendLikePostMessageNotifyAsync(Post post, LikePost react, User userReact)
        {
            try
            {
                var user = new UserIdentifier(post.TenantId, post.CreatorUserId ?? 0);

                var mess = $"{userReact.UserName} đã bày tỏ cảm xúc về bài viết của bạn.";
                var detailUrlApp = $"yoolife://forum/reaction?postId={post.Id}";
                var detailUrlWA = $"/news?id={post.Id}";

                if (!await _onlineClientManager.IsOnlineAsync(user))
                {
                    await SendMessageFireBaseNotify(mess, "", AppNotificationAction.PostCommentAction, new List<long>() { user.UserId }, detailUrlApp, detailUrlWA);
                }

                var messageData = new UserPostMessageNotificationData(
                    AppNotificationAction.PostReactAction,
                    AppNotificationIcon.PostReactIcon,
                    post.Id,
                     null,
                    react.Id,
                    mess,
                    detailUrlApp,
                    detailUrlWA,
                    userReact.ImageUrl,
                    ""
                    
                    );

                await PublishAsync(
                 mess,
                 messageData,
                 severity: NotificationSeverity.Info,
                 userIds: new UserIdentifier[] { user }
                 );

            }
            catch (Exception e)
            {

            }
        }

        private Task SendMessageFireBaseNotify(string message, string title, string action, List<long> userIds, string detailUrlApp, string detailUrlWA, int appType = 0)
        {
            var devicesIds = _fcmTokenRepos.GetAllList(x => x.CreatorUserId.HasValue && userIds.Contains(x.CreatorUserId.Value));

            // send FCM to AppType 
            if (appType != 0)
            {
                devicesIds = devicesIds.Where(x => x.AppType == appType).ToList();
            }
            var tokens = devicesIds.Select(x => x.Token).ToList();
            var a = _cloudMessagingManager.FcmSendToMultiDevice(new FcmMultiSendToDeviceInput()
            {
                Title = title,
                Body = message,
                Data = JsonConvert.SerializeObject(new
                {
                    action = action,
                    detailUrlApp,
                    detailUrlWA
                }),
                Tokens = tokens
            });
            return Task.CompletedTask;
        }

        private async Task PublishAsync(
            string notificationName,
            NotificationData data = null,
            EntityIdentifier entityIdentifier = null,
            NotificationSeverity severity = NotificationSeverity.Info,
            UserIdentifier[] userIds = null,
            UserIdentifier[] excludedUserIds = null,
            int?[] tenantIds = null)
        {
            try
            {
                using (var uow = UnitOfWorkManager.Begin())
                {
                    if (notificationName.IsNullOrEmpty())
                    {
                        throw new ArgumentException("NotificationName can not be null or whitespace!",
                            nameof(notificationName));
                    }

                    if (!tenantIds.IsNullOrEmpty() && !userIds.IsNullOrEmpty())
                    {
                        throw new ArgumentException("tenantIds can be set only if userIds is not set!", nameof(tenantIds));
                    }

                    if (tenantIds.IsNullOrEmpty() && userIds.IsNullOrEmpty())
                    {
                        tenantIds = new[] { AbpSession.TenantId };
                    }

                    var notificationInfo = new NotificationInfo(_guidGenerator.Create())
                    {
                        NotificationName = notificationName,
                        EntityTypeName = entityIdentifier?.Type.FullName,
                        EntityTypeAssemblyQualifiedName = entityIdentifier?.Type.AssemblyQualifiedName,
                        EntityId = entityIdentifier?.Id.ToJsonString(),
                        Severity = severity,
                        UserIds = userIds.IsNullOrEmpty()
                            ? null
                            : userIds.Select(uid => uid.ToUserIdentifierString()).JoinAsString(","),
                        ExcludedUserIds = excludedUserIds.IsNullOrEmpty()
                            ? null
                            : excludedUserIds.Select(uid => uid.ToUserIdentifierString()).JoinAsString(","),
                        TenantIds = GetTenantIdsAsStr(tenantIds),
                        Data = data?.ToJsonString(),
                        DataTypeName = data?.GetType().AssemblyQualifiedName
                    };

                    await _store.InsertNotificationAsync(notificationInfo);

                    await CurrentUnitOfWork.SaveChangesAsync(); //To get Id of the notification

                    if (userIds != null && userIds.Length <= MaxUserCountToDirectlyDistributeANotification)
                    {
                        //We can directly distribute the notification since there are not much receivers
                        await _notificationDistributer.DistributeAsync(notificationInfo.Id);
                    }
                    else
                    {
                        //We enqueue a background job since distributing may get a long time
                        await _backgroundJobManager
                            .EnqueueAsync<NotificationDistributionJob, NotificationDistributionJobArgs>(
                                new NotificationDistributionJobArgs(
                                    notificationInfo.Id
                                )
                            );
                    }

                    await uow.CompleteAsync();
                }
            }
            catch (Exception e)
            {

            }
        }


        /// <summary>
        /// Gets the string for <see cref="NotificationInfo.TenantIds"/>.
        /// </summary>
        /// <param name="tenantIds"></param>
        /// <seealso cref="DefaultNotificationDistributer.GetTenantIds"/>
        private static string GetTenantIdsAsStr(int?[] tenantIds)
        {
            if (tenantIds.IsNullOrEmpty())
            {
                return null;
            }

            return tenantIds
                .Select(tenantId => tenantId == null ? "null" : tenantId.ToString())
                .JoinAsString(",");
        }

        public enum AppType
        {
            APP_USER = 1,
            APP_PARTNER = 2,
        }
    }
}