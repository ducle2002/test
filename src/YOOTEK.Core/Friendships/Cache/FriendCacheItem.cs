using System;
using Abp.AutoMapper;

namespace Yootek.Friendships.Cache
{
    [AutoMapFrom(typeof(Friendship))]
    public class FriendCacheItem
    {
        public const string CacheName = "AppUserFriendCache";

        public long FriendUserId { get; set; }

        public int? FriendTenantId { get; set; }

        public string FriendUserName { get; set; }

        public string FriendTenancyName { get; set; }

        public string FriendImageUrl { get; set; }

        public int UnreadMessageCount { get; set; }

        public bool? IsSender { get; set; }

        public int StateAddFriend { get; set; }

        public FriendshipState State { get; set; }
        public FollowState? FollowState { get; set; }

        public DateTime LastMessageDate { get; set; }
    }
}