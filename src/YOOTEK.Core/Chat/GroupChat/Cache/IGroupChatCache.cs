using Abp;
using IMAX.Friendships;
using IMAX.Friendships.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Chat.Cache
{
    public interface IGroupChatCache
    {
        Task<UserGroupChatCacheItem> GetUserGroupChatCacheItemInternal(UserIdentifier userIdentifier);

        Task<List<FriendCacheItem>> GetMemberGroupChatCacheItem(long id, int? tenantId);

    }
}
