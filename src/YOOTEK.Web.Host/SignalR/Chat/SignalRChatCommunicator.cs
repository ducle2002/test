using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using Abp.AutoMapper;
using Abp.Dependency;
using Abp.ObjectMapping;
using Abp.RealTime;
using Castle.Core.Logging;
using Yootek.Chat;
using Yootek.Chat.Dto;
using Yootek.EntityDb;
using Yootek.Friendships;
using Yootek.Friendships.Dto;
using Yootek.Services;
using Microsoft.AspNetCore.SignalR;

namespace Yootek.Web.Host.Chat
{
    public class SignalRChatCommunicator : IChatCommunicator, ITransientDependency
    {
        /// <summary>
        /// Reference to the logger.
        /// </summary>
        public ILogger Logger { get; set; }
        private readonly IObjectMapper _objectMapper;
        private static IHubContext<ChatHub> _chatHub;
        private readonly IOnlineClientManager _onlineClientManager;

        public SignalRChatCommunicator(
            IOnlineClientManager onlineClientManager,
            IObjectMapper objectMapper,
            IHubContext<ChatHub> chatHub)
        {
            _objectMapper = objectMapper;
            Logger = NullLogger.Instance;
            _onlineClientManager = onlineClientManager;
            _chatHub = chatHub;
        }



        public async Task SendMessageToClient(IReadOnlyList<IOnlineClient> clients, ChatMessage message, ChatMessage messageReplied = null)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    return;
                }
                var mes = _objectMapper.Map<ChatMessageDto>(message);
                if (messageReplied != null)
                {
                    mes.MessageReplied = _objectMapper.Map<ChatMessageDto>(messageReplied);
                }

                await signalRClient.SendAsync("getChatMessage", mes);
            }
        }
        public async Task SendDeleteMessageToClient(IReadOnlyList<IOnlineClient> clients, ChatMessage message)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    return;
                }

                await signalRClient.SendAsync("deleteChatMessage", _objectMapper.Map<ChatMessageDto>(message));
            }
        }


        public async Task SendFriendshipRequestToClient(IReadOnlyList<IOnlineClient> clients, Friendship friendship, bool isOwnRequest, bool isFriendOnline)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    return;
                }

                var friendshipRequest = _objectMapper.Map<FriendDto>(friendship);
                friendshipRequest.IsOnline = isFriendOnline;

                await signalRClient.SendAsync("getFriendshipRequest", friendshipRequest, isOwnRequest);
            }
        }

        public async Task SendUserConnectionChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, bool isConnected)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    continue;
                }

                await signalRClient.SendAsync("getUserConnectNotification", user, isConnected);
            }
        }

        public async Task SendUserStateChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, FriendshipState newState)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    continue;
                }

                await signalRClient.SendAsync("getUserStateChange", user, newState);
            }
        }
        
        public async Task SendUserFollowStateChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, FollowState newState)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    continue;
                }

                await signalRClient.SendAsync("getUserStateChange", user, newState);
            }
        }

        public async Task SendAllUnreadMessagesOfUserReadToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    continue;
                }

                await signalRClient.SendAsync("getallUnreadMessagesOfUserRead", user);
            }
        }

        public async Task SendReadStateChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    continue;
                }

                await signalRClient.SendAsync("SendReadStateChangeToClients", user);
            }
        }

        public async Task SendTyppingMessagesToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    continue;
                }

                await signalRClient.SendAsync("SendTyppingMessagesToClients", user);
            }
        }
        public async Task SendEmotionToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, int type)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    continue;
                }

                await signalRClient.SendAsync("SendEmotionToClients", user);
            }
        }

        private IClientProxy GetSignalRClientOrNull(IOnlineClient client)
        {
            var signalRClient = _chatHub.Clients.Client(client.ConnectionId);
            if (signalRClient == null)
            {
                Logger.Debug("Can not get chat user " + client.UserId + " from SignalR hub!");
                return null;
            }

            return signalRClient;
        }


        #region Chatgroup

        [System.Obsolete]
        public async Task SendMessageToGroupChatClient(string groupChatCode, GroupMessage message, string senderImageUrl)
        {
            var mes = message.MapTo<GroupMessageDto>();
            mes.SenderImageUrl = senderImageUrl;
            await _chatHub.Clients.Group(groupChatCode).SendAsync("sendMessageToGroupChatClient", mes);
        }

        public async Task SendNotificationAddGroupToUserClient(string groupChatCode, UserIdentifier user)
        {
            await _chatHub.Clients.Group(groupChatCode).SendAsync("sendFriendshipRequestToClient", user);
        }

        public async Task SendAllUnreadGroupMessagesOfUserReadToClients(string groupChatCode, UserIdentifier user)
        {
            await _chatHub.Clients.Group(groupChatCode).SendAsync("sendAllUnreadGroupMessagesOfUserReadToClients", user);
        }

        public async Task AddUserToGroupChat(IReadOnlyList<IOnlineClient> clients, string groupChatCode, UserIdentifier user)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    return;
                }
                await _chatHub.Groups.AddToGroupAsync(client.ConnectionId, groupChatCode);

            }
        }


        public async Task SendNotificationCreateGroupToUserClient(string groupChatCode, UserIdentifier user)
        {
            await _chatHub.Clients.Group(groupChatCode).SendAsync("sendNotificationCreateGroupToUserClient", user);
        }

        public async Task RemoveUserFromGroupChat(IReadOnlyList<IOnlineClient> clients, string groupChatCode, UserIdentifier user)
        {
            foreach (var client in clients)
            {
                var signalRClient = GetSignalRClientOrNull(client);
                if (signalRClient == null)
                {
                    return;
                }
                await _chatHub.Groups.RemoveFromGroupAsync(client.ConnectionId, groupChatCode);

            }
        }

        [System.Obsolete]
        public async Task SendDeleteGroupMessageToClient(string groupChatCode, GroupMessage message)
        {
            var mes = message.MapTo<GroupMessageDto>();
            await _chatHub.Clients.Group(groupChatCode).SendAsync("sendDeleteGroupMessageToClient", mes);
        }

        #endregion

    }
}