using Abp;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.RealTime;
using Abp.UI;
using IMAX.Authorization.Users;
using IMAX.Friendships;
using IMAX.Friendships.Cache;
using IMAX.Organizations;
using IMAX.Organizations.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IMAX.Chat
{
    internal class OrganizationUnitChatManager : IMAXDomainServiceBase, IOrganizationUnitChatManager
    {
        private readonly IFriendshipManager _friendshipManager;
        private readonly IChatCommunicator _chatCommunicator;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly UserManager _userManager;
        private readonly ITenantCache _tenantCache;
        private readonly IUserFriendsCache _userFriendsCache;
        private readonly IUserEmailer _userEmailer;
        private readonly IRepository<ChatMessage, long> _chatMessageRepository;
        private readonly IChatFeatureChecker _chatFeatureChecker;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IUserOrganizationUnitCache _userOrganizationUnitCache;
        private readonly IRepository<UserOrganizationUnit, long> _userOrganizationUnitRepos;
        private readonly AppOrganizationUnitManager _appOrganizationUnitManager;

        public OrganizationUnitChatManager(
            IFriendshipManager friendshipManager,
            IChatCommunicator chatCommunicator,
            IOnlineClientManager onlineClientManager,
            IUserOrganizationUnitCache userOrganizationUnitCache,
            UserManager userManager,
            ITenantCache tenantCache,
            IUserFriendsCache userFriendsCache,
            IUserEmailer userEmailer,
            IRepository<ChatMessage, long> chatMessageRepository,
            IChatFeatureChecker chatFeatureChecker,
            IRepository<UserOrganizationUnit, long> userOrganizationUnitRepos,
            IUnitOfWorkManager unitOfWorkManager,
            AppOrganizationUnitManager appOrganizationUnitManager)
        {
            _friendshipManager = friendshipManager;
            _chatCommunicator = chatCommunicator;
            _onlineClientManager = onlineClientManager;
            _userManager = userManager;
            _tenantCache = tenantCache;
            _userFriendsCache = userFriendsCache;
            _userEmailer = userEmailer;
            _chatMessageRepository = chatMessageRepository;
            _chatFeatureChecker = chatFeatureChecker;
            _unitOfWorkManager = unitOfWorkManager;
            _userOrganizationUnitCache = userOrganizationUnitCache;
            _userOrganizationUnitRepos = userOrganizationUnitRepos;
            _appOrganizationUnitManager = appOrganizationUnitManager;
        }

        public async Task DeleteMessageOrgAsync(UserIdentifier sender, UserIdentifier receiver, Guid deviceMessageId, long id)
        {
            //CheckReceiverExists(receiver);
            var message = await _chatMessageRepository.FirstOrDefaultAsync(x => (x.Id == id || x.SharedMessageId == deviceMessageId) && x.Side == ChatSide.Sender);
            if (message == null)
            {
                return;
            }
            await HandleDeleteMessageSenderAsync(sender, message);
            await HandleDeleteMessageReceiverAsync(receiver, message);
        }

        public async Task SendMessageOrgAsync(UserIdentifier user, UserIdentifier organization, string message,string fileUrl, string senderTenancyName, string senderUserName, string senderProfilePictureId, long? messageRepliedId, int typeMessage = 0, bool isAdmin = false)
        {

            var sharedMessageId = Guid.NewGuid();

            await HandleUserToOrgAsync(user, organization, message,fileUrl, sharedMessageId, messageRepliedId, typeMessage, isAdmin);
            await HandleOrgToUserAsync(organization, user, message,fileUrl, sharedMessageId, messageRepliedId, typeMessage, isAdmin);
            // await HandleOrgUserInfoChangeAsync(sender, receiver, senderTenancyName, senderUserName, senderProfilePictureId);
        }

        private async Task HandleUserToOrgAsync(UserIdentifier senderIdentifier, UserIdentifier receiverIdentifier, string message,string fileUrl, Guid sharedMessageId, long? messageRepliedId, int typeMessage, bool isAdmin)
        {
            var friendshipState = (await _friendshipManager.GetFriendshipOrNullAsync(senderIdentifier, receiverIdentifier))?.State;
            if (friendshipState == null && !isAdmin)
            {
                friendshipState = FriendshipState.Accepted;

                var receiverTenancyName = await GetTenancyNameOrNull(receiverIdentifier.TenantId);

                var receiverUser = await _appOrganizationUnitManager.GetAsync(receiverIdentifier.UserId, receiverIdentifier.TenantId);
                await _friendshipManager.CreateFriendshipAsync(
                    new Friendship(
                        senderIdentifier,
                        receiverIdentifier,
                        receiverTenancyName,
                        receiverUser.DisplayName,
                        receiverUser.ImageUrl,
                        friendshipState.Value,
                        true,
                        true
                        )
                );
            }

            ChatMessage sentMessage;
            if (isAdmin)
            {
                sentMessage = new ChatMessage(
                receiverIdentifier,
                senderIdentifier,
                ChatSide.Receiver,
                message,
                ChatMessageReadState.Read,
                sharedMessageId,
                ChatMessageReadState.Unread,
                typeMessage != 0 ? typeMessage : CheckTypeMessage(message),
                messageRepliedId,
                true,
                fileUrl);

                Save(sentMessage);
                var messRep = new ChatMessage();
                if (messageRepliedId != null)
                {
                    messRep = await GetMessageAsync(messageRepliedId.Value, senderIdentifier.UserId, ChatSide.Sender);
                }

                await _chatCommunicator.SendMessageToClient(
                   await _onlineClientManager.GetAllByUserIdAsync(receiverIdentifier),
                    sentMessage,
                    messRep
                    );
            }
            else
            {
                sentMessage = new ChatMessage(
                senderIdentifier,
                receiverIdentifier,
                ChatSide.Sender,
                message,
                ChatMessageReadState.Unread,
                sharedMessageId,
                ChatMessageReadState.Read,
                typeMessage != 0 ? typeMessage : CheckTypeMessage(message),
                messageRepliedId,
                true,
                fileUrl
               );

                Save(sentMessage);
                var messRep = new ChatMessage();
                if (messageRepliedId != null)
                {
                    messRep = await GetMessageAsync(messageRepliedId.Value, senderIdentifier.UserId, ChatSide.Sender);
                }

                await _chatCommunicator.SendMessageToClient(
                   await _onlineClientManager.GetAllByUserIdAsync(senderIdentifier),
                    sentMessage,
                    messRep
                    );
            }


        }

        private async Task HandleOrgToUserAsync(UserIdentifier senderIdentifier, UserIdentifier receiverIdentifier, string message,string fileUrl, Guid sharedMessageId, long? messageRepliedId, int typeMessage, bool isAdmin)
        {
            var friendshipState = (await _friendshipManager.GetFriendshipOrNullAsync(senderIdentifier, receiverIdentifier))?.State;

            if (friendshipState == null && !isAdmin)
            {
                var senderTenancyName = await GetTenancyNameOrNull(senderIdentifier.TenantId);
                //var senderUser = await _appOrganizationUnitManager.GetAsync(receiverIdentifier.UserId, receiverIdentifier.TenantId);
                var senderUser = await _userManager.GetUserAsync(receiverIdentifier);
                await _friendshipManager.CreateFriendshipAsync(
                    new Friendship(
                        senderIdentifier,
                        receiverIdentifier,
                        senderTenancyName,
                        senderUser.UserName,
                        senderUser.ImageUrl,
                        FriendshipState.Accepted,
                        false,
                        true
                    )
                );
            }
            ChatMessage sentMessage;
            if (isAdmin)
            {
                sentMessage = new ChatMessage(
                    receiverIdentifier,
                    senderIdentifier,
                    ChatSide.Sender,
                    message,
                    ChatMessageReadState.Read,
                    sharedMessageId,
                    ChatMessageReadState.Read,
                    typeMessage != 0 ? typeMessage : CheckTypeMessage(message),
                    messageRepliedId,
                    true,
                    fileUrl
                );
                Save(sentMessage);
                var messRep = new ChatMessage();
                if (messageRepliedId != null)
                {
                    messRep = await GetMessageAsync(messageRepliedId.Value, senderIdentifier.UserId, ChatSide.Receiver);
                }
                var clients = await GetOrganizationUnitChatClient(receiverIdentifier.UserId, senderIdentifier.TenantId);
                if (clients.Any())
                {
                    await _chatCommunicator.SendMessageToClient(clients, sentMessage, messRep);
                }
            }
            else
            {
                sentMessage = new ChatMessage(
                senderIdentifier,
                receiverIdentifier,
                ChatSide.Receiver,
                message,
                ChatMessageReadState.Unread,
                sharedMessageId,
                ChatMessageReadState.Unread,
                typeMessage != 0 ? typeMessage : CheckTypeMessage(message),
                messageRepliedId,
                true,
                fileUrl
               );

                Save(sentMessage);
                var messRep = new ChatMessage();
                if (messageRepliedId != null)
                {
                    messRep = await GetMessageAsync(messageRepliedId.Value, senderIdentifier.UserId, ChatSide.Receiver);
                }
                var clients = await GetOrganizationUnitChatClient(senderIdentifier.UserId, senderIdentifier.TenantId);
                if (clients.Any())
                {
                    await _chatCommunicator.SendMessageToClient(clients, sentMessage, messRep);
                }
            }



        }

        private async Task<IReadOnlyList<IOnlineClient>> GetOrganizationUnitChatClient(long orgId, int? tenantId = null)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(tenantId))
                {
                    var orgs = await _userOrganizationUnitRepos.GetAllListAsync(x => x.OrganizationUnitId == orgId);
                    if (orgs == null)
                    {
                        return null;
                    }
                    var result = new List<IOnlineClient>();
                    foreach (var uo in orgs)
                    {
                        var user = new UserIdentifier(tenantId, uo.UserId);
                        var client = await _onlineClientManager.GetAllByUserIdAsync(user);
                        result.AddRange(client);
                    }
                    return result;
                }
            });

        }

        private async Task HandleOrgUserInfoChangeAsync(UserIdentifier sender, UserIdentifier receiver, string senderTenancyName, string senderUserName, string senderProfilePictureId)
        {
            var receiverCacheItem = _userFriendsCache.GetCacheItemOrNull(receiver);

            var senderAsFriend = receiverCacheItem?.Friends.FirstOrDefault(f => f.FriendTenantId == sender.TenantId && f.FriendUserId == sender.UserId);
            if (senderAsFriend == null)
            {
                return;
            }

            if (senderAsFriend.FriendTenancyName == senderTenancyName &&
                senderAsFriend.FriendUserName == senderUserName &&
                senderAsFriend.FriendProfilePictureId == senderProfilePictureId)
            {
                return;
            }

            var friendship = (await _friendshipManager.GetFriendshipOrNullAsync(receiver, sender));
            if (friendship == null)
            {
                return;
            }

            friendship.FriendTenancyName = senderTenancyName;
            friendship.FriendUserName = senderUserName;
            friendship.FriendProfilePictureId = senderProfilePictureId;

            await _friendshipManager.UpdateFriendshipAsync(friendship);
        }

        public async Task HandleDeleteMessageSenderAsync(UserIdentifier sender, ChatMessage message)
        {
            await _chatMessageRepository.DeleteAsync(message.Id);
            var clients = await _onlineClientManager.GetAllByUserIdAsync(sender);
            if (clients.Any())
            {
                await _chatCommunicator.SendDeleteMessageToClient(clients, message);
            }

        }

        public async Task HandleDeleteMessageReceiverAsync(UserIdentifier receiver, ChatMessage message)
        {
            var mes = await _chatMessageRepository.FirstOrDefaultAsync(x => x.SharedMessageId == message.SharedMessageId && x.Side == ChatSide.Receiver);
            if (mes != null)
            {
                await _chatMessageRepository.DeleteAsync(mes.Id);
               // var clients = await _onlineClientManager.GetAllByUserIdAsync(receiver);
                var clients = await GetOrganizationUnitChatClient(receiver.UserId, receiver.TenantId);
                if (clients.Any())
                {
                    await _chatCommunicator.SendDeleteMessageToClient(clients, mes);
                }
            }
        }

        private void CheckReceiverExists(UserIdentifier receiver)
        {
            var receiverUser = _userManager.GetUserOrNull(receiver);
            if (receiverUser == null)
            {
                throw new UserFriendlyException(L("TargetUserNotFoundProbablyDeleted"));
            }
        }


        public Task<ChatMessage> FindMessageAsync(int id, long userId)
        {
            throw new NotImplementedException();
        }

        #region Common
        public virtual long Save(ChatMessage message)
        {
            return _unitOfWorkManager.WithUnitOfWork(() =>
            {
                using (CurrentUnitOfWork.SetTenantId(message.TenantId))
                {
                    return _chatMessageRepository.InsertAndGetId(message);
                }
            });
        }


        private async Task<string> GetTenancyNameOrNull(int? tenantId)
        {
            if (tenantId.HasValue)
            {
                var tenant = await _tenantCache.GetAsync(tenantId.Value);
                return tenant.TenancyName;
            }

            return null;
        }

        private int CheckTypeMessage(string message)
        {
            string regexImg = "([^\\s]+(\\.(?i)(jpe?g|png|gif|bmp))$)";
            var img = Regex.IsMatch(message, regexImg, RegexOptions.Compiled);
            if (img)
            {
                return (int)TypeMessageEnum.Image;
            }
            else
            {
                return (int)TypeMessageEnum.Text;
            }

        }

        private async Task<ChatMessage> GetMessageAsync(long id, long userId, ChatSide side)
        {
            var mes = await _chatMessageRepository.FirstOrDefaultAsync(m => (m.Id == id && m.UserId == userId));
            if (side == ChatSide.Sender)
            {
                return mes;
            }
            else if (side == ChatSide.Receiver && mes != null)
            {
                return await _chatMessageRepository.FirstOrDefaultAsync(m => (m.SharedMessageId == mes.SharedMessageId && m.UserId == userId));
            }
            return null;
        }
        #endregion
    }
}
