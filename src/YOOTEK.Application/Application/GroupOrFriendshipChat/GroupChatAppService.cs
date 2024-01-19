using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.MultiTenancy;
using Abp.RealTime;
using Abp.Runtime.Session;
using Yootek.Application;
using Yootek.Application.RoomOrFriendships.Dto;
using Yootek.Authorization.Users;
using Yootek.Chat;
using Yootek.Chat.Dto;
using Yootek.Common.DataResult;
using Yootek.Friendships;
using Yootek.Friendships.Dto;
using Yootek.GroupChats;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Abp.Application.Friendships
{
    public interface IGroupChatAppService : IApplicationService
    {
        Task<DataResult> AddUserToGroupChat(AddUserGroupChatInput input);
        Task<object> CreateGroupChat(CreateGroupChatInput input);
        Task<object> UpdateInfoGroupChat(UpdateInfoGroupChatInput input);
        Task<DataResult> GetAllGroupChats(GetAllGroupChatInput input);
        Task<object> GetMessageGroupChat(GetMessageGroupChatInput input);
        Task MarkAllUnreadGroupMessagesOfUserAsRead(MarkAllUnreadGroupMessagesOfUserAsReadInput input);
    }

    [AbpAuthorize]
    public class GroupChatAppService : YootekAppServiceBase, IGroupChatAppService
    {

        private readonly IOnlineClientManager _onlineClientManager;
        private readonly IChatCommunicator _chatCommunicator;
        private readonly ITenantCache _tenantCache;
        private readonly IChatFeatureChecker _chatFeatureChecker;
        private readonly IGroupChatManager _groupChatManager;
        private readonly IRepository<Friendship, long> _friendshipRepository;
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly IRepository<GroupChat, long> _groupChatRepository;
        private readonly IRepository<GroupMessage, long> _groupMessageRepository;
        private readonly IRepository<UserGroupChat, long> _groupUserChatRepository;
        private readonly IRepository<User, long> _userRepository;

        public GroupChatAppService(
            IOnlineClientManager onlineClientManager,
            IChatCommunicator chatCommunicator,
            ITenantCache tenantCache,
            IChatFeatureChecker chatFeatureChecker,
            IGroupChatManager groupChatManager,
            IRepository<Friendship, long> friendshipRepository,
            IRepository<ChatMessage, long> chatMessageRepository,
            IRepository<GroupChat, long> groupChatRepository,
            IRepository<GroupMessage, long> groupMessageRepository,
            IRepository<UserGroupChat, long> groupUserChatRepository,
            IRepository<User, long> userRepository)
        {
            _onlineClientManager = onlineClientManager;
            _chatCommunicator = chatCommunicator;
            _tenantCache = tenantCache;
            _chatFeatureChecker = chatFeatureChecker;
            _groupChatManager = groupChatManager;
            _groupChatRepository = groupChatRepository;
            _groupMessageRepository = groupMessageRepository;
            _friendshipRepository = friendshipRepository;
            _chatMessageRepository = chatMessageRepository;
            _groupUserChatRepository = groupUserChatRepository;
            _userRepository = userRepository;
        }

        public async Task<DataResult> AddUserToGroupChat(AddUserGroupChatInput input)
        {
            try
            {

                var userIdentifier = AbpSession.ToUserIdentifier();
                var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());
                var groupchat = await _groupChatRepository.FirstOrDefaultAsync(x => (x.Id == input.GroupId));
                if (input.MemberShips != null && groupchat != null)
                {
                    foreach (var member in input.MemberShips)
                    {
                        var friend = new UserIdentifier(member.TenantId, member.FriendId);
                        if (_groupChatManager.GetMemberGroupChatOrNull(friend, input.GroupId) == null)
                        {
                            await _groupChatManager.AddMembershipGroupChat(friend, input.GroupId);
                        }

                        var clients = await _onlineClientManager.GetAllByUserIdAsync(friend);
                        if (clients.Any())
                        {
                            var isFriendOnline = await _onlineClientManager.IsOnlineAsync(friend);
                            await _chatCommunicator.SendNotificationAddGroupToUserClient(groupchat.GroupChatCode, friend);
                        }
                    };
                }

                return DataResult.ResultSuccess("Update state success !");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                throw;
            }
        }

        public async Task<object> CreateGroupChat(CreateGroupChatInput input)
        {
            try
            {
                var user = AbpSession.ToUserIdentifier();
                var groupChat = new GroupChat(user, input.GroupName, input.GroupImageUrl);
                var groupId = await _groupChatManager.CreatGroupChat(groupChat);
                var initmember = new UserIdentifier(user.TenantId, user.UserId);
                await _groupChatManager.AddMembershipGroupChat(initmember, groupId, GroupChatRole.Leader);
                var userClients = await _onlineClientManager.GetAllByUserIdAsync(user);
                if (userClients.Any())
                {
                    var isFriendOnline = await _onlineClientManager.IsOnlineAsync(user);
                    await _chatCommunicator.AddUserToGroupChat(userClients, groupChat.GroupChatCode, user);
                }

                if (input.MemberShips != null)
                {
                    foreach (var member in input.MemberShips)
                    {
                        var friend = new UserIdentifier(member.FriendTenantId, member.FriendUserId);
                        if (_groupChatManager.GetMemberGroupChatOrNull(friend, groupId) == null)
                        {
                            await _groupChatManager.AddMembershipGroupChat(friend, groupId);
                        }

                        var memberClients = await _onlineClientManager.GetAllByUserIdAsync(friend);
                        if (memberClients.Any())
                        {
                            var isFriendOnline = await _onlineClientManager.IsOnlineAsync(friend);
                            await _chatCommunicator.AddUserToGroupChat(memberClients, groupChat.GroupChatCode, friend);
                        }

                    };

                }
                await _chatCommunicator.SendNotificationCreateGroupToUserClient(groupChat.GroupChatCode, user);
                return DataResult.ResultSuccess("Create success!");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                throw;
            }
        }

      
        public async Task<object> UpdateInfoGroupChat(UpdateInfoGroupChatInput input)
        {
            try
            {
                var group = await _groupChatRepository.FirstOrDefaultAsync(input.Id);
                if (group == null) return DataResult.ResultFail("Room not found !");
                group.GroupImageUrl = input.GroupImageUrl;
                group.Name = input.Name;

                await _groupChatRepository.UpdateAsync(group);

                return DataResult.ResultSuccess("Update success!");

            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<DataResult> GetAllGroupChats(GetAllGroupChatInput input)
        {
            try
            {
                var userId = AbpSession.GetUserId();

                var query = (from gp in _groupChatRepository.GetAll()
                                 join ug in _groupUserChatRepository.GetAll() on gp.Id equals ug.GroupChatId
                                 where ug.UserId == userId
                                 select new GroupChatDto()
                                 {
                                     CreationTime = gp.CreationTime,
                                     Id = gp.Id,
                                     Name = gp.Name,
                                     GroupImageUrl = gp.GroupImageUrl,
                                     TenantId = gp.TenantId,
                                     GroupChatCode = gp.GroupChatCode,
                                     IsGroupOrFriend = true

                                 })
                                 .AsQueryable();

                var allGroups = await query.PageBy(input).ToListAsync();

                foreach (var group in allGroups)
                {
                    var members = _groupUserChatRepository.GetAllList(m => (m.GroupChatId == group.Id && m.UserId != userId));
                   
                    foreach (var member in members)
                    {
                        if (await _onlineClientManager.IsOnlineAsync(new UserIdentifier(member.TenantId, member.UserId)))
                        {
                            group.IsOnline = true;
                        }
                    }

                    group.MemberNumer = members.Count();
                    group.LastMessage = _groupMessageRepository.GetAll()
                       .Where(m => (m.UserId == userId && m.GroupId == group.Id))
                       .OrderByDescending(m => m.CreationTime)
                       .FirstOrDefault();
                    group.LastMessageDate = group.LastMessage != null ? group.LastMessage.CreationTime : group.LastMessageDate;

                    group.UnreadMessageCount = _groupMessageRepository.GetAll()
                     .Where(m => (m.UserId == userId && m.GroupId
                     == group.Id && m.ReadState == ChatMessageReadState.Unread))
                     .OrderByDescending(m => m.CreationTime)
                     .Take(100)
                     .ToList()
                     .Count();
                }

                return DataResult.ResultSuccess(allGroups, "Get group success !", query.Count());
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                throw;
            }
        }

        public async Task<DataResult> GetGroupChatDetail(long id)
        {
            try
            {
                var userId = AbpSession.GetUserId();

                var dt = await _groupChatRepository.FirstOrDefaultAsync(id);

                if(dt == null) return DataResult.ResultFail( "Group not found !");
                var group = ObjectMapper.Map<GroupChatDto>(dt);

                var members = _groupUserChatRepository.GetAllList(m => (m.GroupChatId == group.Id && m.UserId != userId));

                group.Members = ObjectMapper.Map<List<UserGroupChatDto>>(members);
              
                foreach (var member in members)
                {
                    if (await _onlineClientManager.IsOnlineAsync(new UserIdentifier(member.TenantId, member.UserId)))
                    {
                        group.IsOnline = true;
                        break;
                    }
                }
                return DataResult.ResultSuccess(group, "Get group success !");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                throw;
            }
        }

        public async Task<object> GetMessageGroupChat(GetMessageGroupChatInput input)
        {
            try
            {
                var userId = AbpSession.GetUserId();
                var query = (from ms in _groupMessageRepository.GetAll()
                             join us in _userRepository.GetAll() on ms.CreatorUserId equals us.Id into tb_us
                             from us in tb_us
                             where ms.UserId == userId && ms.GroupId == input.GroupChatId
                             select new GroupMessageDto()
                             {
                                 Id = ms.Id,
                                 CreationTime = ms.CreationTime,
                                 CreatorUserId = ms.CreatorUserId,
                                 GroupId = ms.GroupId,
                                 Message = ms.Message,
                                 ReadState = ms.ReadState,
                                 SenderImageUrl = us.ImageUrl,
                                 ShareMessageId = ms.ShareMessageId,
                                 Side = ms.Side,
                                 TenantId = ms.TenantId,
                                 TypeMessage = ms.TypeMessage,
                                 UserId = ms.UserId

                             })
                             .OrderByDescending(m => m.CreationTime)
                             .AsQueryable();
                var messages = await query.PageBy(input)
                             .ToListAsync();


                return DataResult.ResultSuccess(new List<GroupMessageDto>(messages), "Get group success !", query.Count());
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);
                throw;
            }
        }

        public async Task MarkAllUnreadGroupMessagesOfUserAsRead(MarkAllUnreadGroupMessagesOfUserAsReadInput input)
        {
            var userId = AbpSession.GetUserId();
            var messages = await _groupMessageRepository
                .GetAll()
                .Where(m =>
                    m.UserId == userId &&
                    m.GroupId == input.GroupChatId &&
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

        }

    }
}
