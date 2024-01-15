using Abp.AutoMapper;
using Abp.Dependency;
using Abp.RealTime;
using Castle.Core.Logging;
using Yootek.EntityDb;
using Yootek.Notifications.UserBillNotification;
using Yootek.Web.Host.Chat;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Web.Host.SignalR.UserBillNotification
{
    public class UserBillRealtimeNotifier : IUserBillRealtimeNotifier, ITransientDependency
    {
        public ILogger Logger { get; set; }

        private static IHubContext<ChatHub> _notifyHub;
        private readonly IOnlineClientManager _onlineClientManager;

        public UserBillRealtimeNotifier(
            IOnlineClientManager onlineClientManager,
            IHubContext<ChatHub> notifyHub)
        {
            Logger = NullLogger.Instance;
            _onlineClientManager = onlineClientManager;
            _notifyHub = notifyHub;
        }

        public Task NotifyUpdateStateBill(IReadOnlyList<IOnlineClient> clients, UserBill item)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    continue;
                }

                // signalRClient.getUserConnectNotification(user, isConnected);
                _notifyHub.Clients.Client(client.ConnectionId).SendAsync("NotifyUpdateStateBill", item);
            }

            return Task.CompletedTask;
        }

        private dynamic GetSignalRClientOrNull(IOnlineClient client)
        {
            var signalRClient = _notifyHub.Clients.Client(client.ConnectionId);
            if (signalRClient == null)
            {
                Logger.Debug("Can not get user " + client.UserId + " from SignalR hub!");
                return null;
            }

            return signalRClient;
        }
    }
}
