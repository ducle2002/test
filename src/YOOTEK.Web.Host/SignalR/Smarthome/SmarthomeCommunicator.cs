

using Abp.Dependency;
using Abp.RealTime;
using Castle.Core.Logging;
using Yootek.Notifications;
using Yootek.Web.Host.Chat;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace Yootek.Web.Host.SignalR
{
    public class SmarthomeCommunicator : ISmarthomeCommunicator, ITransientDependency
    {
        /// <summary>
        /// Reference to the logger.
        /// </summary>
        public ILogger Logger { get; set; }

        private static IHubContext<ChatHub> _smarthomeHub;
        private readonly IOnlineClientManager _onlineClientManager;

        public SmarthomeCommunicator(
            IOnlineClientManager onlineClientManager,
            IHubContext<ChatHub> smarthomeHub)
        {
            Logger = NullLogger.Instance;
            _onlineClientManager = onlineClientManager;
            _smarthomeHub = smarthomeHub;
        }

        [System.Obsolete]
        public void NotifyUpdateSmarthomeToClient(IReadOnlyList<IOnlineClient> clients, string smarthomecode)
        {

            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    continue;
                }
                // signalRClient.getUserConnectNotification(user, isConnected);
                _smarthomeHub.Clients.Client(client.ConnectionId).SendAsync("NotifyUpdateSmarthome", smarthomecode);
            }

        }


        private dynamic GetSignalRClientOrNull(IOnlineClient client)
        {
            var signalRClient = _smarthomeHub.Clients.Client(client.ConnectionId);
            if (signalRClient == null)
            {
                Logger.Debug("Can not get smarthome user " + client.UserId + " from SignalR hub!");
                return null;
            }

            return signalRClient;
        }

        public void NotifyAddMemberToClient(IReadOnlyList<IOnlineClient> clients, string smarthomecode)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    continue;
                }

                // signalRClient.getUserConnectNotification(user, isConnected);
                _smarthomeHub.Clients.Client(client.ConnectionId).SendAsync("NotifyAddMemberToClient", smarthomecode);
            }
        }
    }
}
