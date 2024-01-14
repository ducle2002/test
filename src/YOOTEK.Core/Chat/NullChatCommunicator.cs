//using Abp;
//using Abp.RealTime;
//using Yootek.EntityDb;
//using Yootek.Friendships;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Yootek.Chat
//{
//    public class NullChatCommunicator : IChatCommunicator
//    {
//        public void SendMessageToClient(IReadOnlyList<IOnlineClient> clients, ChatMessage message)
//        {

//        }

//        public void SendFriendshipRequestToClient(IReadOnlyList<IOnlineClient> clients, Friendship friend, bool isOwnRequest, bool isFriendOnline)
//        {

//        }

//        public void SendUserConnectionChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, bool isConnected)
//        {

//        }

//        public void SendUserStateChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, FriendshipState newState)
//        {

//        }

//        public void SendAllUnreadMessagesOfUserReadToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user)
//        {

//        }

//        public void AddUserToGroupChat(IReadOnlyList<IOnlineClient> clients, string groupChatCode, UserIdentifier user)
//        {

//        }

//        public void SendMessageToGroupChatClient(string groupChatCode, RoomMessage message)
//        {

//        }

//        public void SendNotificationAddGroupToUserClient(string groupChatCode, UserIdentifier user)
//        {

//        }

//        public void SendNotificationCreateGroupToUserClient(string groupChatCode, UserIdentifier user)
//        {

//        }

//        public void SendAllUnreadRoomMessagesOfUserReadToClients(string groupChatCode, UserIdentifier user)
//        {

//        }

//        public void RemoveUserFromGroupChat(IReadOnlyList<IOnlineClient> clients, string groupChatCode, UserIdentifier user)
//        {
//            throw new NotImplementedException();
//        }

//        public void SendNotificaionToClientTenant(IReadOnlyList<IOnlineClient> clients, CommonNotification noti)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
