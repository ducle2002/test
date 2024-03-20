using Abp;
using Abp.Notifications;
using Abp.RealTime;
using Yootek.Authorization.Users;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Notifications
{
    public interface INotificationCommunicator
    {
        Task SendNotifyMaintenanceServer(int type);
        Task SendNotificationRefeshDataAsync(string method, IReadOnlyList<IOnlineClient> clients);
        Task SendNotificationsAsync(string name, NotificationData data, UserIdentifier[] users);
        void SendNotificaionToUserTenant(IReadOnlyList<IOnlineClient> clients, CityNotification noti);
        void SendCommentFeedbackToUserTenant(IReadOnlyList<IOnlineClient> clients, CitizenReflectComment noti);
        void AdminUpdateStateFeedback(IReadOnlyList<IOnlineClient> clients, CitizenReflect noti);
        void SendCommentFeedbackToAdminTenant(IReadOnlyList<UserIdentifier> clients, CitizenReflectComment noti);
        void SendNotificationToAdminTenant(IReadOnlyList<User> clients, CitizenReflect noti);
        Task SendMessageEventAsync(string eventName, object data, UserIdentifier[] users);
    }
}
