using Yootek.Friendships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Organizations.Cache.Dto
{
    public class FriendChatOrganizationUnitCacheItem
    {
        public static string CacheName = "FriendChatOrganizationUnitCacheItem";
        public long FriendUserId { get; set; }

        public int? FriendTenantId { get; set; }

        public string FriendUserName { get; set; }

        public string FriendTenancyName { get; set; }

        public string FriendImageUrl { get; set; }

        public int UnreadMessageCount { get; set; }

        public bool? IsOrganizationUnit { get; set; }

        public int StateAddFriend { get; set; }

        public FriendshipState State { get; set; }
        public FollowState? FollowState { get; set; }
        public DateTime LastMessageDate { get; set; }
    }
}
