using System.Linq;
using Abp;
using Abp.Dependency;
using Abp.RealTime;
using Yootek.Chat;
using Yootek.Friendships.Cache;

namespace Yootek.Friendships
{
    public class ChatUserStateWatcher : ISingletonDependency
    {
        private readonly IChatCommunicator _chatCommunicator;
        private readonly IUserFriendsCache _userFriendsCache;
        private readonly IOnlineClientManager _onlineClientManager;

        public ChatUserStateWatcher(
            IChatCommunicator chatCommunicator,
            IUserFriendsCache userFriendsCache,
            IOnlineClientManager onlineClientManager)
        {
            _chatCommunicator = chatCommunicator;
            _userFriendsCache = userFriendsCache;
            _onlineClientManager = onlineClientManager;
        }

        public void Initialize()
        {
            _onlineClientManager.UserConnected += OnlineClientManager_UserConnected;
            _onlineClientManager.UserDisconnected += OnlineClientManager_UserDisconnected;
        }

        private void OnlineClientManager_UserConnected(object sender, OnlineUserEventArgs e)
        {
            NotifyUserConnectionStateChange(e.User, true);
        }

        private void OnlineClientManager_UserDisconnected(object sender, OnlineUserEventArgs e)
        {
            NotifyUserConnectionStateChange(e.User, false);
        }

        private async void NotifyUserConnectionStateChange(UserIdentifier user, bool isConnected)
        {
            var cacheItem = _userFriendsCache.GetCacheItem(user);

            foreach (var friend in cacheItem.Friends)
            {
                var friendUserClients = await _onlineClientManager.GetAllByUserIdAsync(new UserIdentifier(friend.FriendTenantId, friend.FriendUserId));
                if (!friendUserClients.Any())
                {
                    continue;
                }

                await _chatCommunicator.SendUserConnectionChangeToClients(friendUserClients, user, isConnected);
            }
        }
    }
}