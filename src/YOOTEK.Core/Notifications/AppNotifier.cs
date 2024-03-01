using Abp;
using Abp.BackgroundJobs;
using Abp.Collections.Extensions;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Json;
using Abp.Localization;
using Abp.Notifications;
using Abp.RealTime;
using Abp.Runtime.Session;
using crypto;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.MultiTenancy;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Yootek.Extensions;
using Yootek.Lib.CrudBase;
using static Yootek.Notifications.AppNotifier;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Yootek.Notifications
{
    public class AppNotifier : YootekDomainServiceBase, IAppNotifier
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
        private readonly IRepository<FcmGroups, long> _fcmGroupRepos;
        private readonly IRepository<NotificationInfo, Guid> _notificationInfoRepos;
        private readonly HttpClient _httpClient;

        public AppNotifier(
            INotificationPublisher notificationPublisher,
            INotificationCommunicator notificationCommunicator,
            ICloudMessagingManager cloudMessagingManager,
            INotificationStore store,
            IBackgroundJobManager backgroundJobManager,
            INotificationDistributer notificationDistributer,
            IGuidGenerator guidGenerator,
            IRepository<FcmTokens, long> fcmTokenRepos,
            IRepository<FcmGroups, long> fcmGroupRepos,
            IOnlineClientManager onlineClientManager,
            YootekHttpClient yootekHttpClient,
            IConfiguration configuration
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
            _fcmGroupRepos = fcmGroupRepos; 
            _httpClient = yootekHttpClient.GetHttpClient(configuration["ApiSettings:Yootek.Notification"]);
        }

        public async Task WelcomeToTheApplicationAsync(User user)
        {
            var message = new UserMessageNotificationDataBase(
                    AppNotificationAction.UserWelcomeApp,
                    AppNotificationIcon.UserWelcomeApp,
                    TypeAction.Detail,
                    "Hãy cùng khám phá các tính năng thông minh trên nền tảng xã hội thông minh này nhé! Tham khảo thêm tại: https://yoolife.vn",
                    "yoolife://app/smartsocial-shopping",
                    "yoolife://app/smartsocial-shopping"
                );

            await PublishAsync(
              $"Chào mừng {user.FullName} đã đến với Yoolife !",
              message,
              severity: NotificationSeverity.Success,
              userIds: new[] { user.ToUserIdentifier() }
           );
            await SendMessageFireBaseNotify("Hãy cùng khám phá các tính năng thông minh trên nền tảng xã hội thông minh này nhé! Tham khảo thêm tại: https://yoolife.vn",
                 $"Chào mừng {user.FullName} đã đến với Yoolife !",
                AppNotificationAction.UserWelcomeApp,
                new UserIdentifier[] { user.ToUserIdentifier() },
                    "yoolife://app/smartsocial-shopping",
                    "yoolife://app/smartsocial-shopping");

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
                await SendMessageFireBaseNotify(fireBaseMessage, notificationName, messageNotification.Action, users, detailUrlApp, detailUrlWA);
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
                if ( await _onlineClientManager.IsOnlineAsync(us))
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
                await SendMessageFireBaseNotify(fireBaseMessage, notificationName, messageNotification.Action, users, detailUrlApp, detailUrlWA);
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
            await SendMessageFireBaseNotify(fireBaseMessage, notificationName, messageNotification.Action, users, detailUrlApp, detailUrlWA);

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
                //    if (await _onlineClientManager.IsOnlineAsync(us))
                //    {
                //        userOnlines.Add(us);
                //    }

                //}

                if (users.Length > 0)
                {
                    var userIds = users.Select(x => x.UserId).ToList();
                    await SendMessageFireBaseNotify(fireBaseMessage, notificationName, messageNotification.Action, users, detailUrlApp, detailUrlWA, appType);
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
                    YootekConsts.LocalizationSourceName
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
                    await SendMessageFireBaseNotify(mess, "", AppNotificationAction.PostCommentAction, new UserIdentifier[] {user}, detailUrlApp, detailUrlWA);
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
                    await SendMessageFireBaseNotify(mess, "", AppNotificationAction.PostCommentAction, new UserIdentifier[] { user }, detailUrlApp, detailUrlWA);
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

        public async Task SendUserMessageNotifyToAllUserSocialAsync(string notificationName, string fireBaseMessage, UserMessageNotificationDataBase messageNotification, string detailUrlApp, string detailUrlWA)
        {
            await SendMessageFirebaseAllUser(notificationName, fireBaseMessage, detailUrlApp, detailUrlWA, messageNotification);
        }

        private Task SendMessageFireBaseNotify(string message, string title, string action, UserIdentifier[] users, string detailUrlApp, string detailUrlWA, int appType = 0)
        {
            var devicesIds = new List<FcmTokens>();
            
            foreach(var  user in users)
            {
                using(CurrentUnitOfWork.SetTenantId(user.TenantId))
                {
                    var ids = _fcmTokenRepos.GetAllList(x => x.CreatorUserId.HasValue && user.UserId == x.CreatorUserId.Value);
                    devicesIds.AddRange(ids);
                }

            }
            // send FCM to AppType 
            if (appType != 0)
            {
                devicesIds = devicesIds.Where(x => x.AppType == (EntityDb.AppType)appType).ToList();
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

        private async Task SendMessageFireBaseGroup(string message, string title, string action, string groupName, string detailUrlApp, string detailUrlWA)
        {

            await _cloudMessagingManager.FcmSendToMultiDevice(new FcmMultiSendToDeviceInput()
            {
                Title = title,
                Body = message,
                Data = JsonConvert.SerializeObject(new
                {
                    action = action,
                    detailUrlApp,
                    detailUrlWA
                }),
                GroupName = groupName,
            });

        }

        private async Task SendMessageFirebaseAllUser(string notificationName, string fireBaseMessage, string detailUrlApp, string detailUrlWA,  UserMessageNotificationDataBase messageNotification, NotificationSeverity severity = NotificationSeverity.Info)
        {
            using(CurrentUnitOfWork.SetTenantId(null))
            {
                var fcmtokens = await _fcmTokenRepos.GetAll()
                   .OrderByDescending(x => x.Id)
                   .Where(x => x.CreatorUserId.HasValue && x.CreationTime > DateTime.Now.AddYears(-1)).ToListAsync();
                var tokens = fcmtokens.Select(x => x.Token).Distinct().ToList();
                var users = fcmtokens.Where(us => us.CreatorUserId.HasValue).Select(x => new UserIdentifier(x.TenantId, x.CreatorUserId.Value)).Distinct().ToArray();
               

                if(tokens.Count() > 999)
                {
                    var count = tokens.Count() / 1000 + 1;
                    for(var i = 0; i < count; i ++)
                    {
                        var token2s = tokens.Skip(i *1000).Take(1000).ToList();
                        await _cloudMessagingManager.FcmSendToMultiDevice(new FcmMultiSendToDeviceInput()
                        {
                            Title = notificationName,
                            Body = fireBaseMessage,
                            Data = JsonConvert.SerializeObject(new
                            {
                                action = messageNotification.Action,
                                detailUrlApp,
                                detailUrlWA
                            }),
                            Tokens = token2s
                        });
                    }

                }else
                {
                    await _cloudMessagingManager.FcmSendToMultiDevice(new FcmMultiSendToDeviceInput()
                    {
                        Title = notificationName,
                        Body = fireBaseMessage,
                        Data = JsonConvert.SerializeObject(new
                        {
                            action = messageNotification.Action,
                            detailUrlApp,
                            detailUrlWA
                        }),
                        Tokens = tokens
                    });
                }

                await PublishAsync(
                   notificationName,
                   messageNotification,
                   severity: severity,
                   userIds: users
                   );
            }
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

        public async Task SendMessageNotificationInternalAsync(string notificationName, string fireBaseMessage, string detailUrlApp, string detailUrlWA, UserIdentifier[] users, NotificationData messageData, EntityDb.AppType appType = EntityDb.AppType.ALL, bool isOnlyFirebase = false, bool isSendGroup = false, string groupName = "", NotificationSeverity severity = NotificationSeverity.Info)
        {
            var request = new InternalNotificationInput()
            {
                AppType = appType,
                DetailUrlApp = detailUrlApp,
                DetailUrlWA = detailUrlWA,
                FirebaseMessage = fireBaseMessage,
                GroupName = groupName,
                IsOnlyFirebase = isOnlyFirebase,
                IsSendGroup = isSendGroup,
                NotificationName = notificationName,
                NotificationMessageData = messageData,
                Severity = severity,
                Users = users
            };

            await _httpClient.SendAsync<DataResult>("/api/services/app/InternalNotification/CreateNotification", HttpMethod.Post, request);

        }

        public enum AppType
        {
            APP_USER = 1,
            APP_PARTNER = 2,
        }
    }
}