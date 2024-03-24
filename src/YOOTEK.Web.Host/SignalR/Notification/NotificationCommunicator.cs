using Abp.Dependency;
using Abp.RealTime;
using Castle.Core.Logging;
using Yootek.EntityDb;
using Yootek.Notifications;
using Yootek.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using Abp.AutoMapper;
using Yootek.Web.Host.Chat;
using Yootek.Authorization.Users;
using System.Threading.Tasks;
using Abp.Notifications;
using Abp;

namespace Yootek.Web.Host.SignalR
{
    public class NotificationCommunicator : INotificationCommunicator, ITransientDependency
    {
        /// <summary>
        /// Reference to the logger.
        /// </summary>
        public ILogger Logger { get; set; }

        private static IHubContext<ChatHub> _notificationHub;
        private readonly IOnlineClientManager _onlineClientManager;

        public NotificationCommunicator(
            IOnlineClientManager onlineClientManager,
            IHubContext<ChatHub> notificationHub)
        {
            Logger = NullLogger.Instance;
            _onlineClientManager = onlineClientManager;
            _notificationHub = notificationHub;
        }

        [System.Obsolete]
        public async Task SendNotifyMaintenanceServer(int type)
        {

            await _notificationHub.Clients.All.SendAsync("SendNotifyMaintenanceServer", type);

        }

        [System.Obsolete]
        public void SendNotificaionToUserTenant(IReadOnlyList<IOnlineClient> clients, CityNotification noti)
        {

            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    continue;
                }

                // signalRClient.getUserConnectNotification(user, isConnected);
                _notificationHub.Clients.Client(client.ConnectionId).SendAsync("SendNotificaionToUserTenant", noti.MapTo<CityNotificationDto>());
            }

        }

        [System.Obsolete]
        public async Task SendNotificationRefeshDataAsync(string method, IReadOnlyList<IOnlineClient> clients)
        {

            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    continue;
                }

                // signalRClient.getUserConnectNotification(user, isConnected);
                await _notificationHub.Clients.Client(client.ConnectionId).SendAsync(method, true);
            }

        }

        [System.Obsolete]
        public void SendNotificationToAdminTenant(IReadOnlyList<User> clients, CitizenReflect noti)
        {

            foreach (var client in clients)
            {
                //var signalRClient = GetSignalRClientOrNull(client);
                //if (signalRClient == null)
                //{
                //    continue;
                //}

                // signalRClient.getUserConnectNotification(user, isConnected);
                //_notificationHub.Clients.Client(client.ConnectionId).SendAsync("SendNotificationToAdminTenant", noti.MapTo<UserFeedbackDto>());

                _notificationHub.Clients.User(client.Id.ToString()).SendAsync("SendNotificationToAdminTenant", noti.MapTo<UserFeedbackDto>());

            }
        }

        [System.Obsolete]
        public void AdminUpdateStateFeedback(IReadOnlyList<IOnlineClient> clients, CitizenReflect noti)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    continue;
                }
                _notificationHub.Clients.Client(client.ConnectionId).SendAsync("AdminUpdateStateFeedback", noti.MapTo<UserFeedbackDto>());
            }
        }

        private dynamic GetSignalRClientOrNull(IOnlineClient client)
        {
            var signalRClient = _notificationHub.Clients.Client(client.ConnectionId);
            if (signalRClient == null)
            {
                Logger.Debug("Can not get chat user " + client.UserId + " from SignalR hub!");
                return null;
            }

            return signalRClient;
        }

        [System.Obsolete]
        public void SendCommentFeedbackToUserTenant(IReadOnlyList<IOnlineClient> clients, CitizenReflectComment noti)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    continue;
                }

                // signalRClient.getUserConnectNotification(user, isConnected);
                _notificationHub.Clients.Client(client.ConnectionId).SendAsync("SendCommentFeedbackToUserTenant", noti.MapTo<UserFeedbackCommentDto>());
            }
        }

        [System.Obsolete]
        public void SendCommentFeedbackToAdminTenant(IReadOnlyList<UserIdentifier> clients, CitizenReflectComment noti)
        {
            foreach (var client in clients)
            {
                _notificationHub.Clients.User(client.UserId.ToString()).SendAsync("sendcmfbtoadtenant", noti.MapTo<UserFeedbackCommentDto>());

            }
        }

        public async Task SendNotificationsAsync(string name, NotificationData data, UserIdentifier[] users)
        {

            foreach (var client in users)
            {
                var noti = new UserNotification()
                {
                    TenantId = client.TenantId,
                    UserId = client.UserId,
                    State = UserNotificationState.Unread,
                    Notification = new TenantNotification()
                    {
                        NotificationName = name,
                        Data = data,
                        TenantId = client.TenantId
                    }
                };

                await _notificationHub.Clients.User(client.UserId.ToString()).SendAsync("app.notifications.received", noti);

            }
        }

        public async Task SendMessageEventAsync(string eventName, object data, UserIdentifier[] users)
        {

            foreach (var client in users)
            {
                await _notificationHub.Clients.User(client.UserId.ToString()).SendAsync(eventName, data);

            }
        }
    }
}