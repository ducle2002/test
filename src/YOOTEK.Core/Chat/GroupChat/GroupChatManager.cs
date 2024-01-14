using Abp;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.RealTime;
using Yootek.Authorization.Users;
using Yootek.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.GroupChats
{
    public class GroupChatManager : YootekDomainServiceBase, IGroupChatManager
    {
        private readonly IRepository<GroupChat, long> _groupChatRepository;
        private readonly IRepository<GroupMessage, long> _groupMessageRepository;
        private readonly IRepository<UserGroupChat, long> _groupUserChatRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly IChatCommunicator _chatCommunicator;
        private readonly UserManager _userManager; 

        public GroupChatManager(

            IRepository<GroupChat, long> groupChatRepository,
            IRepository<GroupMessage, long> groupMessageRepository,
            IRepository<UserGroupChat, long> groupUserChatRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IOnlineClientManager onlineClientManager,
            IChatCommunicator chatCommunicator,
            UserManager userManager
            )
        {
            _groupChatRepository = groupChatRepository;
            _groupMessageRepository = groupMessageRepository;
            _userManager = userManager;
            _groupUserChatRepository = groupUserChatRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _onlineClientManager = onlineClientManager;
            _chatCommunicator = chatCommunicator;
        }

        [UnitOfWork]
        public async Task<bool> AddMembershipGroupChat(UserIdentifier member, long groupId, GroupChatRole role = GroupChatRole.Member)
        {
            using (CurrentUnitOfWork.SetTenantId(member.TenantId))
            {
                var user = _userManager.GetUser(member);
                var groupChatUser = new UserGroupChat()
                {
                    UserId = member.UserId,
                    GroupChatId = groupId,
                    TenantId = member.TenantId,
                    MemberAvatarUrl = user.ImageUrl,
                    Role = role,
                    MemberFullName = user.FullName,
                    MemberTenantId = member.TenantId,
                    MemberUserName = user.UserName
                };
                await _groupUserChatRepository.InsertAsync(groupChatUser);
                CurrentUnitOfWork.SaveChanges();
                return true;
            }

        }

        public async Task<List<string>> GetAllGroupCode(UserIdentifier user)
        {
            return _unitOfWorkManager.WithUnitOfWork(() =>
            {
                using (CurrentUnitOfWork.SetTenantId(user.TenantId))
                {
                    var query = (from r in _groupChatRepository.GetAll()
                                 join ru in _groupUserChatRepository.GetAll() on r.Id equals ru.GroupChatId
                                 where ru.UserId == user.UserId
                                 select r.GroupChatCode).ToList();
                    return query;
                }
            });
        }

        public async Task<long> CreatGroupChat(GroupChat group)
        {
            using (CurrentUnitOfWork.SetTenantId(group.TenantId))
            {
                group.GroupChatCode = GenerateCodeGroupChat();
                var id = await _groupChatRepository.InsertAndGetIdAsync(group);
                return id;
            }
        }

        [UnitOfWork]
        public UserGroupChat GetMemberGroupChatOrNull(UserIdentifier member, long groupId)
        {
            using (CurrentUnitOfWork.SetTenantId(member.TenantId))
            {
                return _groupUserChatRepository.FirstOrDefault(user =>
                                    user.UserId == member.UserId &&
                                    user.TenantId == member.TenantId);
            }
        }

        public GroupChat GetGroupChat(long groupId)
        {
            var group = _groupChatRepository.Get(groupId);
            return group;
        }

        public string GenerateCodeGroupChat()
        {
            Random random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string a = new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[random.Next(s.Length)]).ToArray());
            var checkCode = (a).ToString().Trim();

            var group = _groupChatRepository.FirstOrDefault(r => r.GroupChatCode == checkCode);
            if (group != null)
            {
                return GenerateCodeGroupChat();
            }
            else
            {
                return checkCode;
            }

        }

        public async Task SendGroupChatMessage(UserIdentifier sender, long groupId, string message, string senderImageUrl, int typeMessage = 0)
        {
            try
            {
                var groupChat = await _groupChatRepository.GetAsync(groupId);
                if (groupChat == null)
                {
                    return;
                }
                var shareMessageId = Guid.NewGuid();
                await HandleSenderGroupMessage(sender, groupChat, senderImageUrl, message, typeMessage, shareMessageId);
                var members = _groupUserChatRepository.GetAllList(x => x.UserId != sender.UserId && x.GroupChatId == groupId);

                if (members != null)
                {
                    await HandleMemberRecieveGroupMessage(members, sender.UserId, groupChat, message, typeMessage, shareMessageId);
                }
            }
            catch (Exception e)
            {
            }
        }

        public async Task HandleSenderGroupMessage(UserIdentifier sender, GroupChat group, string senderImageUrl, string message, int typeMessage, Guid sharedMessageId)
        {
            var sentMessage = new GroupMessage(
                   sender,
                   group.Id,
                   ChatSide.Sender,
                   message,
                   ChatMessageReadState.Read,
                   typeMessage
               );
            sentMessage.CreatorUserId = sender.UserId;
            sentMessage.ShareMessageId = sharedMessageId;
            Save(sentMessage);
            await _chatCommunicator.SendMessageToGroupChatClient(group.GroupChatCode, sentMessage, senderImageUrl);

        }

        public async Task HandleMemberRecieveGroupMessage(List<UserGroupChat> members, long senderId, GroupChat group, string message, int typeMessage, Guid sharedMessageId)
        {

            foreach (var member in members)
            {
                var sentMessage = new GroupMessage();
                sentMessage.ShareMessageId = sharedMessageId;
                sentMessage.TenantId = group.TenantId;
                sentMessage.Side = ChatSide.Receiver;
                sentMessage.Message = message;
                sentMessage.TypeMessage = typeMessage;
                sentMessage.CreatorUserId = senderId;
                sentMessage.ReadState = ChatMessageReadState.Unread;
                sentMessage.GroupId = group.Id;
                sentMessage.UserId = member.UserId;
                Save(sentMessage);
            }
        }

        protected virtual long Save(GroupMessage message)
        {
            return _unitOfWorkManager.WithUnitOfWork(() =>
            {
                using (CurrentUnitOfWork.SetTenantId(message.TenantId))
                {
                    return _groupMessageRepository.InsertAndGetId(message);
                }
            });
        }

        public async Task RecallMessageGroup(UserIdentifier user, long groupId, Guid deviceMessageId, long messageId)
        {
            //await _unitOfWorkManager.WithUnitOfWorkAsync( async () => 
            //{
            //    using (CurrentUnitOfWork.SetTenantId(user.TenantId))
            //    {

            //    }
            //});
            var message = await _groupMessageRepository.FirstOrDefaultAsync(x => (x.ShareMessageId == deviceMessageId) && x.Side == ChatSide.Sender);

            if (message == null)
            {
                return;
            }
            await _groupMessageRepository.DeleteAsync(x => x.ShareMessageId == deviceMessageId);
            var groupCode = (from rm in _groupChatRepository.GetAll()
                            where rm.Id == groupId
                            select rm.GroupChatCode).FirstOrDefault();
            await _chatCommunicator.SendDeleteGroupMessageToClient(groupCode, message);

        }

        public async Task LeaveGroupChat(UserIdentifier user, string groupCode)
        {
            var group = await _groupChatRepository.FirstOrDefaultAsync(x => x.GroupChatCode == groupCode);
            if(group != null)
            {

                var member = _groupUserChatRepository.FirstOrDefault(x => x.GroupChatId == group.Id && x.UserId == user.UserId);
                if (member == null) return;
                _groupUserChatRepository.Delete(member.Id);
             
            }
        }

        private Task HandAdminLeaveGroup(UserIdentifier admin, GroupChat group)
        {
            return Task.CompletedTask;
        }
    }
}
