﻿using Abp;
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
using IMAX.Application.Chat.Dto;
using IMAX.Chat;
using IMAX.Chat.Dto;
using IMAX.Common.DataResult;
using IMAX.Dto.Interface;
using IMAX.EntityDb;
using IMAX.Friendships;
using IMAX.Friendships.Cache;
using IMAX.Friendships.Dto;
using IMAX.Organizations;
using IMAX.Organizations.AppOrganizationUnits;
using IMAX.Organizations.Cache;
using IMAX.Organizations.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMAX.Abp.Application.Chat.OrganizationUnitChat
{
    public interface IOrganizationUnitChatAppService : IApplicationService
    {
        Task<object> GetOrganizationUnitChatUser(long orgId);
    }

    public class OrganizationUnitChatAppService : IMAXAppServiceBase, IOrganizationUnitChatAppService
    {
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly IRepository<Friendship, long> _friendshipRepos;
        private readonly IRepository<AppOrganizationUnit, long> _organizationRepos;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationRepos;
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
            IRepository<Citizen, long> citizenRepos
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
        }

        public async Task<object> GetOrganizationUnitIdByUser(long? urbanId)
        {
            var ouIds = UserManager.GetAccessibleOperationDepartmentIds();

            try
            {
                var ouOfUrbanIds = new List<long>();
                if (urbanId.HasValue)
                {
                    ouOfUrbanIds = (from ou in _organizationRepos.GetAll()
                                        join ouc in _organizationRepos.GetAll() on ou.ParentId equals ouc.Id
                                        where ou.Type == APP_ORGANIZATION_TYPE.CHAT
                                        && ouc.ParentId == urbanId
                                        select ouc.Id).ToList();
                }
                
                var data = (from ou in _organizationRepos.GetAll()
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
                             .WhereIf(urbanId.HasValue,x => ouOfUrbanIds.Contains(x.ParentId.Value))
                             .ToList();
                return  DataResult.ResultSuccess(data, "Get success");
            }
            catch (Exception e)
            {
                throw new UserFriendlyException(e.Message);
            }
        }
    
        public async Task<object> GetOrganizationUnitChatUser(long orgId)
        {
            try
            {
                var userId = AbpSession.GetUserId();
                var org = await _organizationRepos.FirstOrDefaultAsync(orgId);

                if (org == null)
                {
                    return null;
                }
                var orgs = await _appOrganizationUnitManager.GetAppOrganizationUnitAsync(new GetOrganizationUnitInput()
                {
                    TenantId = AbpSession.TenantId,
                    Type = APP_ORGANIZATION_TYPE.CHAT
                });

                var data = (from apm in orgs
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
                            .ToList();
                foreach (var friend in data)
                {
                    //friend.IsOnline = _onlineClientManager.IsOnline(
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
                return data;

            }
            catch (Exception e)
            {
                throw new UserFriendlyException(e.Message);
            }
        }
      
        public async Task<object> GetOrganizationUnitChatAdmin(long organizationUnitId)
        {
            try
            {
                var query =
                   (from friendship in _friendshipRepos.GetAll()
                    where friendship.UserId == organizationUnitId
                    select new FriendDto()
                    {
                        FriendUserId = friendship.FriendUserId,
                        FriendTenantId = friendship.FriendTenantId,
                        State = friendship.State,
                        FriendUserName = friendship.FriendUserName,
                        FriendTenancyName = friendship.FriendTenancyName,
                        FriendProfilePictureId = friendship.FriendProfilePictureId,
                        IsOrganizationUnit = friendship.IsOrganizationUnit,
                        LastMessageDate = friendship.CreationTime,
                        FriendInfo = (from ctz in _citizenRepos.GetAll()
                                      where friendship.FriendUserId == ctz.AccountId
                                      select ctz).FirstOrDefault()
                    })
                    .Where(x => x.IsOrganizationUnit == true)
                    .AsQueryable();

                var friends = await query.ToListAsync();
                // var cacheItem = _userOrganizationUnitCache.GetFriendChatOrganizationUnit(organizationUnitId, AbpSession.TenantId);

                //var friends = cacheItem.MapTo<List<FriendDto>>();
                var listresults = new List<ChatFriendOrRoomDto>();

                foreach (var friend in friends)
                {

                    friend.IsOnline = await _onlineClientManager.IsOnlineAsync(
                        new UserIdentifier(friend.FriendTenantId, friend.FriendUserId)
                    );

                    friend.UnreadMessageCount = _chatMessageRepository.GetAll()
                     .Where(m => (m.UserId == organizationUnitId && m.TargetUserId == friend.FriendUserId && m.ReadState == ChatMessageReadState.Unread))
                     .OrderByDescending(m => m.CreationTime)
                     .Take(20)
                     .ToList()
                     .Count();
                    friend.LastMessage = _chatMessageRepository.GetAll()
                       .Where(m => (m.UserId == organizationUnitId && m.TargetUserId == friend.FriendUserId))
                       .OrderByDescending(m => m.CreationTime)
                       .FirstOrDefault();
                    friend.LastMessageDate = friend.LastMessage != null ? friend.LastMessage.CreationTime : friend.LastMessageDate;
                }

                listresults = listresults.Concat(friends).ToList();
                listresults = listresults.OrderByDescending(x => x.LastMessageDate).ToList();
                return new GetUserChatFriendsWithSettingsOutput
                {
                    Friends = listresults,
                    ServerTime = Clock.Now,
                    SenderId = AbpSession.UserId.Value
                };

            }
            catch (Exception e)
            {
                throw new UserFriendlyException(e.Message);
            }
        }
      
        public async Task<ListResultDto<ChatMessageDto>> GetUserChatMessages(GetOrganizationChatMessagesInput input)
        {
            try
            {
                input.TenantId = AbpSession.TenantId;
                var messages = _chatMessageRepository.GetAll()
                    .Where(x => x.IsOrganizationUnit == true)
                    .WhereIf(input.MinMessageId.HasValue, m => m.Id < input.MinMessageId.Value)
                    .Where(m => m.UserId == input.OrganizationUnitId && m.TargetTenantId == input.TenantId && m.TargetUserId == input.UserId)
                    .OrderByDescending(m => m.CreationTime)
                    .Take(50)
                    .ToList();

                messages.Reverse();
                var result = messages.MapTo<List<ChatMessageDto>>();
                if (result != null)
                {
                    foreach (var mes in result)
                    {
                        if (mes.MessageRepliedId != null)
                        {
                            var rep = await _chatMessageRepository.FirstOrDefaultAsync(x => x.Id == mes.MessageRepliedId && x.UserId == input.OrganizationUnitId);
                            if (rep != null)
                            {
                                mes.MessageReplied = rep.MapTo<ChatMessageDto>();
                            }
                        }
                    }
                }

                return new ListResultDto<ChatMessageDto>(result);
            }
            catch (Exception e)
            {
                throw new UserFriendlyException(e.Message);
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
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<int> GetCountMessageUnread(MarkAllUnreadMessagesOfUserAsReadInput input)
        {
            var count =  _chatMessageRepository
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
                    m.ReadState == ChatMessageReadState.Unread)
                .ToListAsync();

            if (!messages.Any())
            {
                return;
            }

            foreach (var message in messages)
            {
                message.ChangeReadState(ChatMessageReadState.Read);
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
