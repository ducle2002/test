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
using Yootek.Friendships.Cache;
using Yootek.Chat.Dto;
using Microsoft.EntityFrameworkCore;
using Yootek.Friendships.Dto;
using Yootek.Friendships;
using System;
using Yootek.Dto.Interface;
using Yootek.Chat.Cache;
using Yootek.Organizations.Cache;
using Yootek.EntityDb;
using Yootek.Common.DataResult;
using Yootek.Application.Chat.Dto;
using Yootek.Authorization.Users;
using Abp.UI;
using Abp.Json;
using Microsoft.AspNetCore.Mvc;

namespace Yootek.Chat
{
    public class ChatAppService : YootekAppServiceBase, IChatAppService
    {
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly IRepository<GroupMessage, long> _roomMessageRepos;
        private readonly IRepository<User, long> _userRepository;
        private readonly IUserFriendsCache _userFriendsCache;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly IChatCommunicator _chatCommunicator;
        private readonly IGroupChatCache _groupChatCache;
        private readonly IUserOrganizationUnitCache _userOrganizationUnitCache;
        private readonly IChatMessageManager _chatMessageManager;

        public ChatAppService(
            IRepository<ChatMessage, long> chatMessageRepository,
            IRepository<GroupMessage, long> roomMessageRepos,
            IRepository<User, long> userRepository,
            IUserFriendsCache userFriendsCache,
            IOnlineClientManager onlineClientManager,
            IChatCommunicator chatCommunicator,
            IGroupChatCache groupChatCache,
            IUserOrganizationUnitCache userOrganizationUnitCache,
            IChatMessageManager chatMessageManager
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
            _chatMessageManager = chatMessageManager;
        }


        public async Task<bool> SendMessage(SendChatMessageInput input)
        {
            var sender = AbpSession.ToUserIdentifier();
            var receiver = new UserIdentifier(input.TenantId, input.UserId);
            try
            {
                await _chatMessageManager.SendMessageAsync(sender, receiver, input.Message, input.FileUrl, input.TenancyName, input.UserName, input.SenderImageUrl, input.MessageRepliedId, input.TypeMessage);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warn("Could not send chat message to user: " + receiver);
                Logger.Warn(ex.ToString(), ex);
                return false;
            }
        }

        [HttpPost]
        public async Task<bool> DeleteMessage(DeleteChatMessageInput input)
        {
            var sender = AbpSession.ToUserIdentifier();
            var receiver = new UserIdentifier(input.TenantId, input.UserId);

            try
            {
                using (AbpSession.Use(AbpSession.GetTenantId(), AbpSession.GetUserId()))
                {
                    await _chatMessageManager.DeleteMessageAsync(sender, receiver, input.SharedMessageId, input.Id);
                    return true;
                }
            }
            catch (UserFriendlyException ex)
            {
                Logger.Warn("Could not send chat message to user: " + receiver);
                Logger.Warn(ex.ToString(), ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Warn("Could not send chat message to user: " + receiver);
                Logger.Warn(ex.ToString(), ex);
                return false;
            }
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
                throw;
            }
        }

        [DisableAuditing]
        public async Task<DataResult> GetUserChatFriendsWithSettings(GetUserChatFriendsWithSettingInput input)
        {
            try
            {
                var userId = AbpSession.GetUserId();
                //var cacheItem = _userFriendsCache.GetCacheItem(AbpSession.ToUserIdentifier());
                var cacheItem = _userFriendsCache.GetUserFriendsCacheItemInternal(AbpSession.ToUserIdentifier(), null);

                if (!string.IsNullOrWhiteSpace(input.Keyword))
                {
                    cacheItem.Friends = cacheItem.Friends.Where(x => x.FriendUserName.Contains(input.Keyword)).ToList();
                }

                var friends = ObjectMapper.Map<List<FriendDto>>(cacheItem.Friends.Skip(input.SkipCount).Take(input.MaxResultCount).ToList());
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

                    friend.State = _userRepository.FirstOrDefault(friend.FriendUserId) == null ? FriendshipState.IsDeleted : friend.State;
                    if (friend.State == FriendshipState.IsDeleted)
                    {
                        friend.FriendUserName = "Người dùng yoolife";
                        friend.FriendImageUrl = null;
                    }
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

                return DataResult.ResultSuccess(listresults, "get success", cacheItem.Friends.Count());

            }
            catch (Exception e)
            {
                Logger.Fatal("GetUserChatFriendsWithSettings : " + e.ToJsonString());
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

                var item = cacheItem.Friends.FirstOrDefault(x => x.FriendUserId == input.UserId && x.FriendTenantId == input.TenantId);

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

        [DisableAuditing]
        public async Task<DataResult> GetUserChatMessages(GetUserChatMessagesInput input)
        {
            try
            {
                var userId = AbpSession.GetUserId();
                input.TenantId = AbpSession.TenantId;
                using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                {
                    var query = _chatMessageRepository.GetAll()
                       .WhereIf(input.IsOrganizationUnit == null || !input.IsOrganizationUnit.Value, m => m.IsOrganizationUnit != true)
                       .WhereIf(input.IsOrganizationUnit.HasValue && input.IsOrganizationUnit.Value, m => m.IsOrganizationUnit == input.IsOrganizationUnit)
                       .WhereIf(input.MinMessageId.HasValue, m => m.Id < input.MinMessageId.Value)
                       .Where(m => m.UserId == userId && m.TargetTenantId == input.TenantId && m.TargetUserId == input.UserId)
                       .OrderByDescending(m => m.CreationTime)
                       .AsQueryable();
                    var messages = await query.PageBy(input)
                       .ToListAsync();

                    messages.Reverse();
                    var result = ObjectMapper.Map<List<ChatMessageDto>>(messages);
                    if (result != null)
                    {
                        foreach (var mes in result)
                        {
                            if (mes.MessageRepliedId != null)
                            {
                                var rep = await _chatMessageRepository.FirstOrDefaultAsync(x => x.Id == mes.MessageRepliedId);
                                if (rep != null)
                                {
                                    mes.MessageReplied = ObjectMapper.Map<ChatMessageDto>(rep);
                                }
                            }
                        }
                    }

                    return DataResult.ResultSuccess(result, "", query.Count());
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

            _userFriendsCache.ResetUnreadMessageCount(userIdentifier, friendIdentifier);

            var onlineClients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
            if (onlineClients.Any())
            {
                await _chatCommunicator.SendAllUnreadMessagesOfUserReadToClients(onlineClients, friendIdentifier);
            }
        }
    }
    public class SendChatMessageInput
    {
        public int? TenantId { get; set; }
        public long? MessageRepliedId { get; set; }
        public long UserId { get; set; }

        public long SenderId { get; set; }

        public string UserName { get; set; }

        public string TenancyName { get; set; }

        public string SenderImageUrl { get; set; }
        public string Message { get; set; }
        public string FileUrl { get; set; }
        public int TypeMessage { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class DeleteChatMessageInput
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public Guid SharedMessageId { get; set; }
        public long UserId { get; set; }
    }

}