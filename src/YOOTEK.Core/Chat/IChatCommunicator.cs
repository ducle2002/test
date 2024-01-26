using Abp;
using Abp.RealTime;
using Yootek.EntityDb;
using Yootek.Friendships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Chat
{
    public interface IChatCommunicator
    {


        #region Chatp2p
        Task SendMessageToClient(IReadOnlyList<IOnlineClient> clients, ChatMessage message, ChatMessage messageReplied = null);
        Task SendDeleteMessageToClient(IReadOnlyList<IOnlineClient> clients, ChatMessage message);

        Task SendFriendshipRequestToClient(IReadOnlyList<IOnlineClient> clients, Friendship friend, bool isOwnRequest, bool isFriendOnline);

        Task SendUserConnectionChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, bool isConnected);

        Task SendUserStateChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, FriendshipState newState);
        
        Task SendUserFollowStateChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, FollowState newState);


        Task SendAllUnreadMessagesOfUserReadToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user);

        Task SendReadStateChangeToClients(IReadOnlyList<IOnlineClient> onlineFriendClients, UserIdentifier user);

        Task SendTyppingMessagesToClients(IReadOnlyList<IOnlineClient> onlineFriendClients, UserIdentifier user);
        Task SendEmotionToClients(IReadOnlyList<IOnlineClient> onlineFriendClients, UserIdentifier user, int type);
        #endregion

        #region GroupChat
        Task AddUserToGroupChat(IReadOnlyList<IOnlineClient> clients, string groupChatCode, UserIdentifier user);

        Task RemoveUserFromGroupChat(IReadOnlyList<IOnlineClient> clients, string groupChatCode, UserIdentifier user);

        Task SendMessageToGroupChatClient(string groupChatCode, GroupMessage message, string senderImageUrl);

        Task SendNotificationAddGroupToUserClient(string groupChatCode, UserIdentifier user);

        Task SendNotificationCreateGroupToUserClient(string groupChatCode, UserIdentifier user);
        Task SendDeleteGroupMessageToClient(string groupChatCode, GroupMessage message);
        Task SendAllUnreadGroupMessagesOfUserReadToClients(string groupChatCode, UserIdentifier user);
        #endregion
    }
}
