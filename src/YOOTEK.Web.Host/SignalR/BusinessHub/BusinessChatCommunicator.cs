using Yootek.Chat.BusinessChat;
using Castle.Core.Logging;
using Abp.ObjectMapping;
using Microsoft.AspNetCore.SignalR;
using Abp.RealTime;
using System.Threading.Tasks;
using System.Collections.Generic;
using Yootek.Application.BusinessChat.Dto;
using Abp;
using Abp.Dependency;
using Yootek.Chat;
using Yootek.Web.Host.Chat;

namespace Yootek.Web.Host.SignalR.BusinessHub
{
    public class BusinessChatCommunicator : IBusinessChatCommunicator, ITransientDependency
    {
        public ILogger Logger { get; set; }
        private readonly IObjectMapper _objectMapper;
        private static IHubContext<ChatHub> _businessHub;
        private readonly IOnlineClientManager _onlineClientManager;

        public BusinessChatCommunicator(
            IOnlineClientManager onlineClientManager,
            IObjectMapper objectMapper,
            IHubContext<ChatHub> businessHub)
        {
            _objectMapper = objectMapper;
            Logger = NullLogger.Instance;
            _onlineClientManager = onlineClientManager;
            _businessHub = businessHub;
        }


        public async Task SendMessageToClient(IReadOnlyList<IOnlineClient> clients, BusinessChatMessage message, BusinessChatMessage messageReplied = null)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    return;
                }
                var mes = _objectMapper.Map<BusinessChatMessageDto>(message);
                if (messageReplied != null)
                {
                    mes.MessageReplied = _objectMapper.Map<BusinessChatMessageDto>(messageReplied);
                }

                await signalRClient.SendAsync("getBusinessChatMessage", mes);
            }
        }

        private IClientProxy GetSignalRClientOrNull(IOnlineClient client)
        {
            var signalRClient = _businessHub.Clients.Client(client.ConnectionId);
            if (signalRClient == null)
            {
                Logger.Debug("Can not get chat user " + client.UserId + " from SignalR hub!");
                return null;
            }

            return signalRClient;
        }

        public async Task SendAllUnreadBusinessChatMessageAsReadToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    continue;
                }

                await signalRClient.SendAsync("getAllUnreadBusinessChatMessageAsRead", user);
            }
        }

        public async Task SendDeleteBusinessMessageToClient(IReadOnlyList<IOnlineClient> clients, BusinessChatMessage mess)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    return;
                }

                await signalRClient.SendAsync("deleteBusinessChatMessage", _objectMapper.Map<BusinessChatMessageDto>(mess));
            }
        }
    }
}
