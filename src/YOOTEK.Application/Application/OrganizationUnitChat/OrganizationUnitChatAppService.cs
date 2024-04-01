using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.RealTime;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using Yootek.Application.Chat.Dto;
using Yootek.Chat;
using Yootek.Chat.Dto;
using Yootek.Common.DataResult;
using Yootek.Dto.Interface;
using Yootek.EntityDb;
using Yootek.Friendships;
using Yootek.Friendships.Cache;
using Yootek.Friendships.Dto;
using Yootek.Organizations;
using Yootek.Organizations.AppOrganizationUnits;
using Yootek.Organizations.Cache;
using Yootek.Organizations.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NPOI.SS.Formula.Functions;
using Yootek.Authorization;
using Yootek.QueriesExtension;
using Nest;
using Yootek.Application;
using Yootek.Services.Dto;
using Yootek.Services;
using Yootek.Yootek.Services.Yootek.SmartCommunity.Count;
using YOOTEK.Application.OrganizationUnitChat.Dto;
using Abp.Linq.Extensions;
using Abp.Json;
using Yootek.Authorization.Users;

namespace Yootek.Abp.Application.Chat.OrganizationUnitChat
{
    public interface IOrganizationUnitChatAppService : IApplicationService
    {
    }

    public class OrganizationUnitChatAppService : YootekAppServiceBase, IOrganizationUnitChatAppService
    {
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly IRepository<Friendship, long> _friendshipRepos;
        private readonly IRepository<AppOrganizationUnit, long> _organizationRepos;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationRepos;
        private readonly IRepository<User, long> _userRepository;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly IChatCommunicator _chatCommunicator;
        private readonly IUserFriendsCache _userFriendsCache;
        private readonly IUserOrganizationUnitCache _userOrganizationUnitCache;
        private readonly AppOrganizationUnitManager _appOrganizationUnitManager;
        private readonly IRepository<Citizen, long> _citizenRepos;

        public OrganizationUnitChatAppService(
            IRepository<ChatMessage, long> chatMessageRepository,
            IUserFriendsCache userFriendsCache,
            IRepository<AppOrganizationUnit, long> organizationRepos,
            IOnlineClientManager onlineClientManager,
            IChatCommunicator chatCommunicator,
            IUserOrganizationUnitCache userOrganizationUnitCache,
            IRepository<Friendship, long> friendshipRepos,
            IRepository<UserOrganizationUnit, long> userOrganizationRepos,
            AppOrganizationUnitManager appOrganizationUnitManager,
            IRepository<Citizen, long> citizenRepos,
            IRepository<User, long> userRepository
            )
        {
            _userFriendsCache = userFriendsCache;
            _chatMessageRepository = chatMessageRepository;
            _onlineClientManager = onlineClientManager;
            _chatCommunicator = chatCommunicator;
            _userOrganizationUnitCache = userOrganizationUnitCache;
            _organizationRepos = organizationRepos;
            _friendshipRepos = friendshipRepos;
            _userOrganizationRepos = userOrganizationRepos;
            _appOrganizationUnitManager = appOrganizationUnitManager;
            _citizenRepos = citizenRepos;
            _userRepository = userRepository;
        }

        public async Task<object> GetOrganizationUnitIdByUser(GetOrganizationUnitIdByUserInput input)
        {
            var ouIds = UserManager.GetAccessibleOperationDepartmentIds();

            try
            {
                var ouOfUrbanIds = new List<long>();
                if (input.UrbanId.HasValue)
                {
                    ouOfUrbanIds = (from ou in _organizationRepos.GetAll()
                                    join ouc in _organizationRepos.GetAll() on ou.ParentId equals ouc.Id
                                    where ou.Type == APP_ORGANIZATION_TYPE.CHAT
                                    && ouc.ParentId == input.UrbanId.Value
                                    select ouc.Id).ToList();
                }

                var query = (from ou in _organizationRepos.GetAll()
                             select new AppOrganizationUnitDto()
                             {
                                 DisplayName = ou.DisplayName,
                                 Id = ou.ParentId.Value,
                                 Type = ou.Type,
                                 TenantId = ou.TenantId,
                                 ParentId = ou.ParentId,
                                 Description = ou.Description,
                                 ImageUrl = ou.ImageUrl

                             })
                             .Where(x => x.Type == APP_ORGANIZATION_TYPE.CHAT)
                             .Where(x => ouIds.Contains(x.ParentId.Value))
                             .WhereIf(input.UrbanId.HasValue, x => ouOfUrbanIds.Contains(x.ParentId.Value))
                             .AsQueryable();
                var data = await query.PageBy(input).ToListAsync();
                return DataResult.ResultSuccess(data, "Get success", query.Count());
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> GetOrganizationUnitChatUser(GetOrganizationUnitChatUserInput input)
        {
            try
            {
                var userId = AbpSession.GetUserId();
                var org = await _organizationRepos.FirstOrDefaultAsync(input.OrganizationUnitId);

                if (org == null)
                {
                    return null;
                }
                var orgs = await _appOrganizationUnitManager.GetAppOrganizationUnitAsync(new GetOrganizationUnitInput()
                {
                    TenantId = AbpSession.TenantId,
                    Type = APP_ORGANIZATION_TYPE.CHAT
                });

                var query = (from apm in orgs
                             where apm.Type == APP_ORGANIZATION_TYPE.CHAT
                             && apm.Code.StartsWith(org.Code)
                             select new TenantProjectChatDto()
                             {
                                 OrganizationUnitId = apm.ParentId,
                                 Name = apm.DisplayName,
                                 ImageUrl = apm.ImageUrl,
                                 Type = apm.Type,
                                 Description = apm.Description

                             })
                            .AsQueryable();
                var data = query.ToList();
                foreach (var friend in data)
                {
                    //friend.IsOnline = await _onlineClientManager.IsOnlineAsync(
                    //    new UserIdentifier(friend.FriendTenantId, friend.FriendUserId)
                    //);

                    friend.UnreadMessageCount = _chatMessageRepository.GetAll()
                     .Where(m => (m.UserId == userId && m.TargetUserId == friend.OrganizationUnitId && m.ReadState == ChatMessageReadState.Unread))
                     .OrderByDescending(m => m.CreationTime)
                     .Take(20)
                     .ToList()
                     .Count();
                    friend.LastMessage = _chatMessageRepository.GetAll()
                       .Where(m => (m.UserId == userId && m.TargetUserId == friend.OrganizationUnitId))
                       .OrderByDescending(m => m.CreationTime)
                       .FirstOrDefault();
                    friend.LastMessageDate = friend.LastMessage != null ? friend.LastMessage.CreationTime : friend.LastMessageDate;
                }

                return DataResult.ResultSuccess(data, "", query.Count());

            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> GetOrganizationUnitChatAdmin(GetOrganizationUnitChatUserInput input)
        {
            try
            {
                var query =
                   (from friendship in _friendshipRepos.GetAll()
                    where friendship.UserId == input.OrganizationUnitId
                    select new FriendDto()
                    {
                        FriendUserId = friendship.FriendUserId,
                        FriendTenantId = friendship.FriendTenantId,
                        State = friendship.State,
                        FollowState = friendship.FollowState,
                        FriendUserName = friendship.FriendUserName,
                        FriendTenancyName = friendship.FriendTenancyName,
                        FriendImageUrl = friendship.FriendImageUrl,
                        IsOrganizationUnit = friendship.IsOrganizationUnit,
                        LastMessageDate = friendship.CreationTime,
                        FriendInfo = (from ctz in _citizenRepos.GetAll()
                                      where friendship.FriendUserId == ctz.AccountId
                                      select ctz).FirstOrDefault()
                    })
                    .Where(x => x.IsOrganizationUnit == true)
                    .AsQueryable();

                var friends = query.ToList();
                // var cacheItem = _userOrganizationUnitCache.GetFriendChatOrganizationUnit(organizationUnitId, AbpSession.TenantId);

                //var friends = cacheItem.MapTo<List<FriendDto>>();
                var listresults = new List<ChatFriendOrRoomDto>();

                foreach (var friend in friends)
                {

                    friend.IsOnline = await _onlineClientManager.IsOnlineAsync(
                        new UserIdentifier(friend.FriendTenantId, friend.FriendUserId)
                    );

                    friend.UnreadMessageCount = _chatMessageRepository.GetAll()
                     .Where(m => (m.UserId == input.OrganizationUnitId && m.TargetUserId == friend.FriendUserId && m.ReadState == ChatMessageReadState.Unread))
                     .OrderByDescending(m => m.CreationTime)
                     .Take(20)
                     .ToList()
                     .Count();
                    friend.LastMessage = _chatMessageRepository.GetAll()
                       .Where(m => (m.UserId == input.OrganizationUnitId && m.TargetUserId == friend.FriendUserId))
                       .OrderByDescending(m => m.CreationTime)
                       .FirstOrDefault();
                    friend.LastMessageDate = friend.LastMessage != null ? friend.LastMessage.CreationTime : friend.LastMessageDate;
                }

                listresults = listresults.Concat(friends).ToList();
                listresults = listresults.OrderByDescending(x => x.LastMessageDate).Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

                return DataResult.ResultSuccess(listresults, "", query.Count());

            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<DataResult> GetUserChatMessages(GetOrganizationChatMessagesInput input)
        {
            try
            {
                input.TenantId = AbpSession.TenantId;
                var query = _chatMessageRepository.GetAll()
                    .Where(x => x.IsOrganizationUnit == true)
                    .WhereIf(input.MinMessageId.HasValue, m => m.Id < input.MinMessageId.Value)
                    .Where(m => m.UserId == input.OrganizationUnitId && m.TargetTenantId == input.TenantId && m.TargetUserId == input.UserId)
                    .OrderByDescending(m => m.CreationTime)
                    .AsQueryable();
                var messages = query.PageBy(input)
                    .ToList();

                messages.Reverse();
                var result = ObjectMapper.Map<List<ChatMessageDto>>(messages);
                if (result != null)
                {
                    foreach (var mes in result)
                    {
                        if (mes.MessageRepliedId != null)
                        {
                            var rep = await _chatMessageRepository.FirstOrDefaultAsync(x => x.Id == mes.MessageRepliedId && x.UserId == input.OrganizationUnitId);
                            if (rep != null)
                            {
                                mes.MessageReplied = ObjectMapper.Map<ChatMessageDto>(rep);
                            }
                        }
                    }
                }

                return DataResult.ResultSuccess(result, "", query.Count());
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public async Task<DataResult> GetUserChat(GetUserChatInput input)
        {
            try
            {
                var userId = AbpSession.GetUserId();
                //var cacheItem = _userFriendsCache.GetCacheItem(AbpSession.ToUserIdentifier());
                var cacheItem = _userFriendsCache.GetUserFriendsCacheItemInternal(AbpSession.ToUserIdentifier(), null);

                var item = cacheItem.Friends.FirstOrDefault(x => x.FriendUserId == input.UserId || x.FriendTenantId == input.TenantId);

                if (item == null)
                {
                    using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                    {
                        item = _userRepository.GetAll().Where(x => x.Id == input.UserId)
                       .Select(x => new FriendCacheItem()
                       {
                           FriendUserId = input.UserId,
                           FriendImageUrl = x.ImageUrl,
                           FriendTenantId = input.TenantId,
                           FriendUserName = x.FullName,
                           State = FriendshipState.Stranger
                       })
                       .FirstOrDefault();
                    }
                }

                if (item == null) return DataResult.ResultSuccess("get success");

                var friend = ObjectMapper.Map<FriendDto>(item);

                friend.IsOnline = await _onlineClientManager.IsOnlineAsync(
                         new UserIdentifier(friend.FriendTenantId, friend.FriendUserId)
                     );

                friend.State = _userRepository.FirstOrDefault(friend.FriendUserId) == null ? FriendshipState.IsDeleted : friend.State;

                if (friend.State == FriendshipState.IsDeleted)
                {
                    friend.FriendUserName = "Người dùng yoolife";
                    friend.FriendImageUrl = null;
                }

                return DataResult.ResultSuccess(friend, "get success");

            }
            catch (Exception e)
            {
                Logger.Fatal("GetUserChatFriendsWithSettings : " + e.ToJsonString());
                throw;
            }
        }

        public async Task<object> GetChatMsgCountStatistics()
        {
            try
            {
                var count = await _chatMessageRepository.GetAll().CountAsync();
                return DataResult.ResultSuccess(count, "Get success!");
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetCountChatOrganizationStatistics()
        {
            try
            {
                List<long> buIds = UserManager.GetAccessibleBuildingOrUrbanIds();
                var query = (from mes in _chatMessageRepository.GetAll()
                             select new ChatMessageStatic
                             {
                                 IsOrganizationUnit = mes.IsOrganizationUnit,
                                 UrbanId = _userOrganizationRepos.GetAll().Where(x => x.UserId == mes.UserId).Select(x => x.OrganizationUnitId).FirstOrDefault(),
                                 BuildingId = _userOrganizationRepos.GetAll().Where(x => x.UserId == mes.UserId).Select(x => x.OrganizationUnitId).FirstOrDefault(),
                             })
                             .WhereByBuildingOrUrbanIf(!IsGranted(IOCPermissionNames.Data_Admin), buIds)
                             .Where(x => x.IsOrganizationUnit == true).CountAsync();

                var result = await query;
                return DataResult.ResultSuccess(result, "Get success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<int> GetCountMessageUnread(MarkAllUnreadMessagesOfUserAsReadInput input)
        {
            var count = _chatMessageRepository
               .GetAll()
               .Where(m => m.IsOrganizationUnit == true)
               .Where(m =>
                   m.TargetTenantId == input.TenantId &&
                   m.TargetUserId == input.UserId &&
                   m.ReadState == ChatMessageReadState.Unread)
               .Count();
            return count;

        }

        public async Task MarkAllUnreadMessagesOfUserAsRead(MarkAllUnreadMessagesOfUserAsReadInput input)
        {
            var userId = AbpSession.GetUserId();
            var messages = await _chatMessageRepository
                .GetAll()
                .Where(m => m.IsOrganizationUnit == true)
                .Where(m =>
                    m.UserId == input.SenderId &&
                    m.TargetTenantId == input.TenantId &&
                    m.TargetUserId == input.UserId &&
                   (m.ReadState == ChatMessageReadState.Unread || m.ReceiverReadState == ChatMessageReadState.Unread))
                .ToListAsync();

            if (!messages.Any())
            {
                return;
            }

            foreach (var message in messages)
            {
                message.ChangeReadState(ChatMessageReadState.Read);
                message.ChangeReceiverReadState(ChatMessageReadState.Read);
            }

            var userIdentifier = AbpSession.ToUserIdentifier();
            var friendIdentifier = input.ToUserIdentifier();


            var onlineClients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
            if (onlineClients.Any())
            {
                await _chatCommunicator.SendAllUnreadMessagesOfUserReadToClients(onlineClients, friendIdentifier);
            }
        }
    }
}
