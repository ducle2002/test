using Abp;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Yootek.Friendships;
using Yootek.Friendships.Cache;
using Yootek.GroupChats;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Chat.Cache
{
    public class UserGroupChatCache : IGroupChatCache, ISingletonDependency
    {
        private readonly IRepository<GroupChat, long> _groupChatRepository;
        private readonly IRepository<UserGroupChat, long> _groupUserChatRepository;
        private readonly IRepository<Friendship, long> _friendShipRepository;
        public UserGroupChatCache(

            IRepository<GroupChat, long> groupChatRepository,
            IRepository<UserGroupChat, long> groupUserChatRepository,
            IRepository<Friendship, long> friendshipRepository
            )
        {

            _groupChatRepository = groupChatRepository;
            _friendShipRepository = friendshipRepository;
            _groupUserChatRepository = groupUserChatRepository;
        }

        public async Task<List<FriendCacheItem>> GetMemberGroupChatCacheItem(long id, int? tenantId)
        {
            var query = (from guc in _groupUserChatRepository.GetAll()
                         join fr in _friendShipRepository.GetAll() on guc.UserId equals fr.UserId
                         where guc.GroupChatId == id
                         select new FriendCacheItem()
                         {

                         }).AsQueryable();
            return await query.ToListAsync();
        }

        public async Task<UserGroupChatCacheItem> GetUserGroupChatCacheItemInternal(UserIdentifier userIdentifier)
        {
            var userId = userIdentifier.UserId;

            var query = (from gp in _groupChatRepository.GetAll()
                         join ugp in _groupUserChatRepository.GetAll() on gp.Id equals ugp.GroupChatId
                         where ugp.UserId == userId
                         select new GroupChatCacheItem()
                         {
                             CreationTime = gp.CreationTime,
                             Name = gp.Name,
                             GroupImageUrl = gp.GroupImageUrl,
                             TenantId = gp.TenantId,
                             GroupChatCode = gp.GroupChatCode


                         }).AsQueryable();
            var allGroups = await query.ToListAsync();

            return new UserGroupChatCacheItem
            {
                TenantId = userIdentifier.TenantId,
                UserId = userIdentifier.UserId,
                GroupChats = allGroups
            };
        }
    }
}
