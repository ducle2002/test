using System.Collections.Generic;
using Abp.Domain.Repositories;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.AutoMapper;
using Abp.Linq.Extensions;
using Abp.RealTime;
using Abp.Runtime.Session;
using Abp.Timing;
using IMAX.Friendships.Cache;
using IMAX.Chat.Dto;
using Microsoft.EntityFrameworkCore;
using IMAX.Friendships.Dto;
using IMAX.Friendships;
using System;
using IMAX.Dto.Interface;
using IMAX.Chat.Cache;
using IMAX.Organizations.Cache;
using IMAX.EntityDb;
using IMAX.Common.DataResult;
using IMAX.Application.Chat.Dto;
using IMAX.Authorization.Users;
using Abp.UI;

namespace IMAX.Chat
{
    public class ChatAppService : IMAXAppServiceBase, IChatAppService
    {
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly IRepository<GroupMessage, long> _roomMessageRepos;
        private readonly IRepository<User, long> _userRepository;
        private readonly IUserFriendsCache _userFriendsCache;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly IChatCommunicator _chatCommunicator;
        private readonly IGroupChatCache _groupChatCache;
        private readonly IUserOrganizationUnitCache _userOrganizationUnitCache;

        public ChatAppService(
            IRepository<ChatMessage, long> chatMessageRepository,
            IRepository<GroupMessage, long> roomMessageRepos,
            IRepository<User, long> userRepository,
            IUserFriendsCache userFriendsCache,
            IOnlineClientManager onlineClientManager,
            IChatCommunicator chatCommunicator,
            IGroupChatCache groupChatCache,
            IUserOrganizationUnitCache userOrganizationUnitCache
            )
        {
            _chatMessageRepository = chatMessageRepository;
            _roomMessageRepos = roomMessageRepos;
            _userRepository = userRepository;
            _userFriendsCache = userFriendsCache;
            _onlineClientManager = onlineClientManager;
            _chatCommunicator = chatCommunicator;
            _groupChatCache = groupChatCache;
            _userOrganizationUnitCache = userOrganizationUnitCache;
        }

        public async Task<DataResult> CountMessageUnreadUser()
        {
            var userId = AbpSession.GetUserId();
            var unreadMessageCount = _chatMessageRepository.GetAll()
                  .Where(m => (m.UserId == userId && m.ReadState == ChatMessageReadState.Unread))
                  .OrderByDescending(m => m.CreationTime)
                  .Take(100)
                  .ToList()
                  .Count();
            var unreadMessageGroupCount = _roomMessageRepos.GetAll()
                  .Where(m => (m.UserId == userId && m.ReadState == ChatMessageReadState.Unread))
                  .OrderByDescending(m => m.CreationTime)
                  .Take(100)
                  .ToList()
                  .Count();
            var data = DataResult.ResultSuccess(unreadMessageCount + unreadMessageGroupCount, "Get success !");
            return data;
        }

      
        public async Task<DataResult> SearchUserMessageByKeyword(SearchMessageInput input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var userId = AbpSession.GetUserId();
                var listresults = new List<ChatFriendOrRoomDto>();
                var messages = await _chatMessageRepository.GetAll()
                       .Where(m => m.UserId == userId)
                       .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Message.Contains(input.Keyword))
                       .OrderByDescending(m => m.CreationTime)
                       .PageBy(input)
                       .ToListAsync();
                if (messages != null && messages.Count > 0)
                {
                    var cacheItem = _userFriendsCache.GetUserFriendsCacheItemInternal(AbpSession.ToUserIdentifier(), FriendshipState.Accepted);
                    foreach (var mes in messages)
                    {
                        var friend = cacheItem.Friends.FirstOrDefault(x => x.FriendUserId == mes.TargetUserId && x.FriendTenantId == mes.TenantId);
                        if (friend != null)
                        {
                            var friendDto = friend.MapTo<FriendDto>();

                            friendDto.IsOnline = await _onlineClientManager.IsOnlineAsync(new UserIdentifier(friend.FriendTenantId, friend.FriendUserId));
                            friendDto.LastMessage = mes;
                            friendDto.LastMessageDate = mes.CreationTime;
                            listresults.Add(friendDto);
                        }
                    }
                }

                var data = DataResult.ResultSuccess(listresults, "Insert success !");
                mb.statisticMetris(t1, 0, "is_administrative");

                return data;

            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw new UserFriendlyException(e.Message);
            }
        }


        [DisableAuditing]
        public async Task<GetUserChatFriendsWithSettingsOutput> GetUserChatFriendsWithSettings(GetUserChatFriendsWithSettingInput input)
        {
            try
            {
                var userId = AbpSession.GetUserId();
                //var cacheItem = _userFriendsCache.GetCacheItem(AbpSession.ToUserIdentifier());
                var cacheItem = _userFriendsCache.GetUserFriendsCacheItemInternal(AbpSession.ToUserIdentifier(), FriendshipState.Accepted);

                if (!string.IsNullOrWhiteSpace(input.Keyword))
                {
                    cacheItem.Friends = cacheItem.Friends.Where(x => x.FriendUserName.Contains(input.Keyword)).ToList();
                }

                var friends = cacheItem.Friends.MapTo<List<FriendDto>>();
                var listresults = new List<ChatFriendOrRoomDto>();

                foreach (var friend in friends)
                {
                    friend.IsOnline = await _onlineClientManager.IsOnlineAsync(
                        new UserIdentifier(friend.FriendTenantId, friend.FriendUserId)
                    );

                    friend.UnreadMessageCount = _chatMessageRepository.GetAll()
                     .Where(m => (m.UserId == userId && m.TargetUserId == friend.FriendUserId && m.ReadState == ChatMessageReadState.Unread))
                     .OrderByDescending(m => m.CreationTime)
                     .Take(20)
                     .ToList()
                     .Count();
                    friend.LastMessage = _chatMessageRepository.GetAll()
                       .Where(m => (m.UserId == userId && m.TargetUserId == friend.FriendUserId))
                       .OrderByDescending(m => m.CreationTime)
                       .FirstOrDefault();
                    friend.LastMessageDate = friend.LastMessage != null ? friend.LastMessage.CreationTime : friend.LastMessageDate;

                    friend.IsBlockOrDelete = _userRepository.FirstOrDefault(friend.FriendUserId) == null ? true : false;
                }

                listresults = listresults.Concat(friends).ToList();

                #region Room chat

                //var roomCaches = await _groupChatCache.GetUserGroupChatCacheItemInternal(AbpSession.ToUserIdentifier());
                //var rooms = roomCaches.GroupChats.MapTo<List<GroupChatDto>>();

                //if(rooms != null)
                //{
                //    foreach(var room in rooms)
                //    {
                //        room.IsOnline = true;
                //        room.IsGroupOrFriend = true;
                //        //room.LastMessage = _roomMessageRepos.GetAll()
                //        // .Where(m => m.UserId == userId)
                //        // .OrderByDescending(m => m.CreationTime)
                //        // .FirstOrDefault();
                //        //room.LastMessageDate = room.LastMessage != null ? room.LastMessage.CreationTime : room.LastMessageDate;
                //        //room.Members = _groupChatCache.GetMemberGroupChatCacheItem(room.Id, room.TenantId).MapTo<List<FriendDto>>();
                //    }
                //}
                //  listresults = listresults.Concat(rooms).ToList();
                listresults = listresults.OrderByDescending(x => x.LastMessageDate).ToList();

                #endregion

                return new GetUserChatFriendsWithSettingsOutput
                {
                    Friends = listresults,
                    ServerTime = Clock.Now,
                    SenderId = AbpSession.UserId.Value
                };
            }
            catch (Exception e)
            {
                throw new UserFriendlyException("GetUserChatFriendsWithSetting exception !" + e.Message);
            }
        }

        [DisableAuditing]
        public async Task<GetUserChatFriendsWithSettingsOutput> GetFriendRequestingList()
        {
            var userId = AbpSession.GetUserId();
            //var cacheItem = _userFriendsCache.GetCacheItem(AbpSession.ToUserIdentifier());
            var cacheItem = _userFriendsCache.GetUserFriendsCacheItemInternal(AbpSession.ToUserIdentifier(), FriendshipState.Requesting, false);

            var friends = cacheItem.Friends.MapTo<List<FriendDto>>();

            foreach (var friend in friends)
            {
                friend.IsOnline = await _onlineClientManager.IsOnlineAsync(
                    new UserIdentifier(friend.FriendTenantId, friend.FriendUserId)
                );

                friend.UnreadMessageCount = _chatMessageRepository.GetAll()
                 .Where(m => (m.UserId == userId && m.TargetUserId == friend.FriendUserId && m.ReadState == ChatMessageReadState.Unread))
                 .OrderByDescending(m => m.CreationTime)
                 .Take(10)
                 .ToList()
                 .Count();
            }
            var listresults = new List<ChatFriendOrRoomDto>();
            listresults = listresults.Concat(friends).ToList();
            return new GetUserChatFriendsWithSettingsOutput
            {
                Friends = listresults,
                ServerTime = Clock.Now
            };
        }

        [DisableAuditing]
        public async Task<ListResultDto<ChatMessageDto>> GetUserChatMessages(GetUserChatMessagesInput input)
        {
            try
            {
                var userId = AbpSession.GetUserId();
                input.TenantId = AbpSession.TenantId;
                using(CurrentUnitOfWork.SetTenantId(input.TenantId))
                {
                    var messages = await _chatMessageRepository.GetAll()
                       .WhereIf(input.IsOrganizationUnit == null || !input.IsOrganizationUnit.Value, m => m.IsOrganizationUnit != true)
                       .WhereIf(input.IsOrganizationUnit.HasValue && input.IsOrganizationUnit.Value, m => m.IsOrganizationUnit == input.IsOrganizationUnit)
                       .WhereIf(input.MinMessageId.HasValue, m => m.Id < input.MinMessageId.Value)
                       .Where(m => m.UserId == userId && m.TargetTenantId == input.TenantId && m.TargetUserId == input.UserId)
                       .OrderByDescending(m => m.CreationTime)
                       .Take(50)
                       .ToListAsync();

                    messages.Reverse();
                    var result = messages.MapTo<List<ChatMessageDto>>();
                    if (result != null)
                    {
                        foreach (var mes in result)
                        {
                            if (mes.MessageRepliedId != null)
                            {
                                var rep = await _chatMessageRepository.FirstOrDefaultAsync(x => x.Id == mes.MessageRepliedId && x.UserId == userId);
                                if (rep != null)
                                {
                                    mes.MessageReplied = rep.MapTo<ChatMessageDto>();
                                }
                            }
                        }
                    }

                    return new ListResultDto<ChatMessageDto>(result);
                }
            }
            catch (Exception e)
            {
                throw new UserFriendlyException("GetUserChatMessages exception !" + e.Message);
            }
        }

        public async Task MarkAllUnreadMessagesOfUserAsRead(MarkAllUnreadMessagesOfUserAsReadInput input)
        {
            var userId = AbpSession.GetUserId();
            var messages = await _chatMessageRepository
                .GetAll()
                .WhereIf(input.IsOrganizationUnit == null, m => m.IsOrganizationUnit != true)
                .WhereIf(input.IsOrganizationUnit.HasValue, m => m.IsOrganizationUnit == input.IsOrganizationUnit)
                .Where(m =>
                    m.UserId == userId &&
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

            _userFriendsCache.ResetUnreadMessageCount(userIdentifier, friendIdentifier);

            var onlineClients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
            if (onlineClients.Any())
            {
                await _chatCommunicator.SendAllUnreadMessagesOfUserReadToClients(onlineClients, friendIdentifier);
            }
        }
    }
}