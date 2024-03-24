using Abp;
using Abp.Localization;
using Abp.Notifications;
using Yootek.Authorization.Users;
using Yootek.EntityDb;
using System.Threading.Tasks;

namespace Yootek.Notifications
{
    public interface IAppNotifier
    {
        Task WelcomeToTheApplicationAsync(User user);
        Task SendMessageNotificationInternalAsync(
            string notificationName, 
            string fireBaseMessage,
            string detailUrlApp,
            string detailUrlWA,
            UserIdentifier[] users,
            NotificationData messageData,
            AppType appType = AppType.ALL,
            bool isOnlyFirebase = false,
            bool isSendGroup = false,
            string groupName = "",
            NotificationSeverity severity = NotificationSeverity.Info
            );
        Task SendMessageNotificationInternalSellerAsync(
            string notificationName,
            string fireBaseMessage,
            string detailUrlApp,
            string detailUrlWA,
            UserIdentifier[] users,
            NotificationData messageData,
            long? providerId,
            EntityDb.AppType appType = EntityDb.AppType.ALL,
            bool isOnlyFirebase = false, 
            bool isSendGroup = false,
            string groupName = "",
            NotificationSeverity severity = NotificationSeverity.Info);

        Task MultiSendMessageAsync(string typeMessage, UserIdentifier[] users, string message, bool isRealtimeMessage = false, bool isCloudMessage = false, bool isSendAll = false, NotificationSeverity severity = NotificationSeverity.Info);
        //Gửi thông báo Firebase
        Task SendUserMessageNotifyOnlyFirebaseAsync(string notificationName, string fireBaseMessage, UserIdentifier[] users, UserMessageNotificationDataBase messageNotification, string detailUrlApp, string detailUrlWA);

        //Chỉ gửi thông báo lưu
        Task SendUserMessageOnlySaveAsync(string notificationName, UserIdentifier[] users, UserMessageNotificationDataBase messageNotification, NotificationSeverity severity = NotificationSeverity.Info);
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
        Task SendUserMessageNotifyFireBaseAsync(string notificationName, string fireBaseMessage, string detailUrlApp, string detailUrlWA, UserIdentifier[] users, UserMessageNotificationDataBase messageNotification, NotificationSeverity severity = NotificationSeverity.Info);

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
        Task SendUserMessageNotifyFireBaseCheckOnlineAsync(string notificationName, string fireBaseMessage, string detailUrlApp, string detailUrlWA, UserIdentifier[] users, UserMessageNotificationDataBase messageNotification, NotificationSeverity severity = NotificationSeverity.Info);

        // Gửi thông báo cả firebase và realtime
        /// <summary>
        /// Send a notification to a list of users, if they are online, send them a notification realtime,
        /// send all them a notification via Firebase
        /// </summary>
        /// <param name="notificationName">The name of the notification.</param>
        /// <param name="fireBaseMessage">The message that will be sent to the user's device.</param>
        /// <param name="users">The users to send the notification to.</param>
        /// <param name="NotificationData">This is the data that will be sent to the client.</param>
        /// <param name="NotificationSeverity">This is the type of notification.</param>
        Task SendUserMessageNotifyFullyAsync(string notificationName, string fireBaseMessage, string detailUrlApp, string detailUrlWA, UserIdentifier[] users, UserMessageNotificationDataBase messageNotification, int appType = 0, NotificationSeverity severity = NotificationSeverity.Info);

        Task SendCommentPostMessageNotifyAsync(Post post, PostComment comment, User userReact);

        Task SendLikePostMessageNotifyAsync(Post post, LikePost react, User userReact);

        Task SendUserMessageNotifyToAllUserSocialAsync(string notificationName, string fireBaseMessage, UserMessageNotificationDataBase messageNotification, string detailUrlApp, string detailUrlWA);

    }
}
