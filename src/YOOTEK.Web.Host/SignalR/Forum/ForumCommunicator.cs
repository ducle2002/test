using Abp.Dependency;
using Abp.RealTime;
using Castle.Core.Logging;
using IMAX.EntityDb;
using IMAX.Forums;
using IMAX.Notifications;
using IMAX.Services.Dto;
using IMAX.Web.Host.Chat;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace IMAX.Web.Host.SignalR
{
    public class ForumCommunicator : IForumCommunicator, ITransientDependency
    {
        /// <summary>
        /// Reference to the logger.
        /// </summary>
        public ILogger Logger { get; set; }

        private static IHubContext<ChatHub> _businessHub;
        private readonly IOnlineClientManager _onlineClientManager;

        public ForumCommunicator(
            IOnlineClientManager onlineClientManager,
            IHubContext<ChatHub> businessHub)
        {
            Logger = NullLogger.Instance;
            _onlineClientManager = onlineClientManager;
            _businessHub = businessHub;
        }


        private dynamic GetSignalRClientOrNull(IOnlineClient client)
        {
            var signalRClient = _businessHub.Clients.Client(client.ConnectionId);
            if (signalRClient == null)
            {
                Logger.Debug("Can not get chat user " + client.UserId + " from SignalR hub!");
                return null;
            }

            return signalRClient;
        }
    }
}