using Abp;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.RealTime;
using Abp.UI;
using Yootek.Authorization.Users;
using Yootek.Friendships;
using Yootek.Friendships.Cache;
using Yootek.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yootek.Organizations;
using Yootek.EntityDb;
using YOOTEK.Common;

namespace Yootek.Chat
{
    public interface IChatMessageManager : IDomainService
    {
        Task SendMessageAsync(UserIdentifier sender, UserIdentifier receiver, string message,string fileUrl, string senderTenancyName, string senderUserName, string senderImageUrl, long? messageRepliedId, int TypeMessage = 0);
        Task DeleteMessageAsync(UserIdentifier sender, UserIdentifier receiver, Guid deviceMessageId, long id);
        long Save(ChatMessage message);
        int GetUnreadMessageCount(UserIdentifier userIdentifier, UserIdentifier sender);
        Task<ChatMessage> FindMessageAsync(int id, long userId);

    }

    [AbpAuthorize]
    public class ChatMessageManager : YootekDomainServiceBase, IChatMessageManager
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
        private readonly IAppNotifier _appNotifier;

        public ChatMessageManager(
            IFriendshipManager friendshipManager,
            IChatCommunicator chatCommunicator,
            IOnlineClientManager onlineClientManager,
            UserManager userManager,
            ITenantCache tenantCache,
            IUserFriendsCache userFriendsCache,
            IUserEmailer userEmailer,
            IRepository<ChatMessage, long> chatMessageRepository,
            IChatFeatureChecker chatFeatureChecker,
            IUnitOfWorkManager unitOfWorkManager,
            IAppNotifier appNotifier
            )
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
            _appNotifier = appNotifier;
        }

        public async Task DeleteMessageAsync(UserIdentifier sender, UserIdentifier receiver, Guid deviceMessageId, long id)
        {
            using(var uow = UnitOfWorkManager.Begin())
            {
                CheckReceiverExists(receiver);
                var message = await _chatMessageRepository.FirstOrDefaultAsync(x => (x.Id == id || x.SharedMessageId == deviceMessageId) && x.Side == ChatSide.Sender);
                if (message == null)
                {
                    return;
                }
                await HandleDeleteMessageSenderAsync(sender, message);
                await HandleDeleteMessageReceiverAsync(receiver, message);
                await uow.CompleteAsync();
            }

        }

        public async Task SendMessageAsync(UserIdentifier sender, UserIdentifier receiver, string message,string fileUrl, string senderTenancyName, string senderUserName, string senderImageUrl, long? messageRepliedId, int typeMessage = 0)
        {
            using (var uow = UnitOfWorkManager.Begin())
            {
                CheckReceiverExists(receiver);

                var friendshipState = (await _friendshipManager.GetFriendshipOrNullAsync(sender, receiver))?.State;
                if (friendshipState == FriendshipState.Blocked)
                {
                    throw new UserFriendlyException(L("UserIsBlocked"));
                }

                var sharedMessageId = Guid.NewGuid();

                await HandleSenderToReceiverAsync(sender, receiver, message, fileUrl, sharedMessageId, messageRepliedId, typeMessage);
                await HandleReceiverToSenderAsync(sender, receiver, message, fileUrl, sharedMessageId, messageRepliedId, typeMessage);
                await HandleSenderUserInfoChangeAsync(sender, receiver, senderTenancyName, senderUserName, senderImageUrl);
                await uow.CompleteAsync();
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

        public virtual int GetUnreadMessageCount(UserIdentifier sender, UserIdentifier receiver)
        {
            return _unitOfWorkManager.WithUnitOfWork(() =>
            {
                using (CurrentUnitOfWork.SetTenantId(receiver.TenantId))
                {
                    return _chatMessageRepository.Count(cm => cm.UserId == receiver.UserId &&
                                                              cm.TargetUserId == sender.UserId &&
                                                              cm.TargetTenantId == sender.TenantId &&
                                                              cm.ReadState == ChatMessageReadState.Unread);
                }
            });
        }

        public async Task<ChatMessage> FindMessageAsync(int id, long userId)
        {
            return await _chatMessageRepository.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        }

        // Lưu và gửi tin nhắn cho chính người gửi
        private async Task HandleSenderToReceiverAsync(UserIdentifier senderIdentifier, UserIdentifier receiverIdentifier, string message,string fileUrl, Guid sharedMessageId, long? messageRepliedId, int typeMessage)
        {
            var friend = await _friendshipManager.GetFriendshipOrNullAsync(senderIdentifier, receiverIdentifier);
            var friendshipState = friend?.State;
            if (friend == null)
            {

                var receiverTenancyName = await GetTenancyNameOrNull(receiverIdentifier.TenantId);
                friendshipState = FriendshipState.Stranger;
                var receiverUser = await _userManager.GetUserAsync(receiverIdentifier);
                friend = new Friendship(
                        senderIdentifier,
                        receiverIdentifier,
                        receiverTenancyName,
                        receiverUser.UserName,
                        receiverUser.ImageUrl,
                        friendshipState.Value);
                await _friendshipManager.CreateFriendshipAsync(friend);
            }

            if (friendshipState == FriendshipState.Blocked)
            {
                return;
            }

            var sentMessage = new ChatMessage(
                senderIdentifier,
                receiverIdentifier,
                ChatSide.Sender,
                message,
                ChatMessageReadState.Read,
                sharedMessageId,
                ChatMessageReadState.Unread,
                typeMessage != 0 ? typeMessage : CheckTypeMessage(message),
                messageRepliedId,
                false,
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

        // Lưu và gửi tin nhắn cho người nhận
        private async Task HandleReceiverToSenderAsync(UserIdentifier senderIdentifier, UserIdentifier receiverIdentifier, string message,string fileUrl, Guid sharedMessageId, long? messageRepliedId, int typeMessage)
        {
            var friend = await _friendshipManager.GetFriendshipOrNullAsync(receiverIdentifier, senderIdentifier);
            var friendshipState = friend?.State;

            if (friend == null)
            {
                var senderTenancyName = await GetTenancyNameOrNull(senderIdentifier.TenantId);

                var senderUser = await _userManager.GetUserAsync(senderIdentifier);

                friend = new Friendship(
                      receiverIdentifier,
                      senderIdentifier,
                      senderTenancyName,
                      senderUser.UserName,
                      senderUser.ImageUrl,
                      FriendshipState.Stranger
                  );
                await _friendshipManager.CreateFriendshipAsync(friend );
            }

            if (friendshipState == FriendshipState.Blocked)
            {
                //Do not send message if receiver banned the sender
                return;
            }

            var sentMessage = new ChatMessage(
                    receiverIdentifier,
                    senderIdentifier,
                    ChatSide.Receiver,
                    message,
                    ChatMessageReadState.Unread,
                    sharedMessageId,
                    ChatMessageReadState.Read,
                    typeMessage != 0 ? typeMessage : CheckTypeMessage(message),
                    messageRepliedId,
                    false,
                    fileUrl
                );

            Save(sentMessage);
            var messRep = new ChatMessage();
            if (messageRepliedId != null)
            {
                messRep = await GetMessageAsync(messageRepliedId.Value, senderIdentifier.UserId, ChatSide.Receiver);
            }
            var clients = await _onlineClientManager.GetAllByUserIdAsync(receiverIdentifier);
            if (clients.Any())
            {
                await _chatCommunicator.SendMessageToClient(clients, sentMessage, messRep);
            }
            await FireNotificationMessageToUserAsync(sentMessage, receiverIdentifier, senderIdentifier , friend);
        }

        // Thay đổi info người gửi nếu có thay đổi
        private async Task HandleSenderUserInfoChangeAsync(UserIdentifier sender, UserIdentifier receiver, string senderTenancyName, string senderUserName, string friendImageUrl)
        {
            var receiverCacheItem = _userFriendsCache.GetCacheItemOrNull(receiver);

            var senderAsFriend = receiverCacheItem?.Friends.FirstOrDefault(f => f.FriendTenantId == sender.TenantId && f.FriendUserId == sender.UserId);
            if (senderAsFriend == null)
            {
                return;
            }

            if (senderAsFriend.FriendTenancyName == senderTenancyName &&
                senderAsFriend.FriendUserName == senderUserName &&
                senderAsFriend.FriendImageUrl == friendImageUrl)
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
            friendship.FriendImageUrl = friendImageUrl;

            senderAsFriend.FriendTenancyName = senderTenancyName;
            senderAsFriend.FriendUserName = senderUserName;
            senderAsFriend.FriendImageUrl = friendImageUrl;

            await _friendshipManager.UpdateFriendshipAsync(friendship);
            _userFriendsCache.UpdateFriend(receiver, senderAsFriend);
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
                var clients = await _onlineClientManager.GetAllByUserIdAsync(receiver);
                if (clients.Any())
                {
                    await _chatCommunicator.SendDeleteMessageToClient(clients, mes);
                }
            }
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

        #region Common
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

        private async Task FireNotificationMessageToUserAsync(ChatMessage message, UserIdentifier sender, UserIdentifier user, Friendship friend)
        {
            try
            {
                var messageData = new UserMessageNotificationDataBase(
                         AppNotificationAction.ChatMessage,
                         AppNotificationIcon.ChatMessageIcon,
                         TypeAction.Detail,
                         message.Message,
                         AppRouterLinks.AppUser_ChatUser + "/" + sender.ToUserIdentifierStringNoti(),
                         AppRouterLinks.AppUser_ChatUser + "/" + sender.ToUserIdentifierStringNoti(),
                         friend.FriendImageUrl
                         );
                await _appNotifier.SendMessageNotificationInternalAsync(
                    friend.FriendUserName + " " + NotificationMessageCheckType(message),
                    message.Message,
                    AppRouterLinks.AppUser_ChatUser + "/" + sender.ToUserIdentifierStringNoti(),
                    AppRouterLinks.AppUser_ChatUser + "/" + sender.ToUserIdentifierStringNoti(),
                    new[] { user },
                    messageData,
                    AppType.USER
                   );
            }catch { }
        }

        private string NotificationMessageCheckType(ChatMessage mes)
        {
            if (mes.TypeMessage == null) return "";
            switch(mes.TypeMessage.Value)
            {
                case (int)TypeMessageEnum.File:
                case (int)TypeMessageEnum.Files:
                    return "đã gửi file !";
                case (int)TypeMessageEnum.Text:
                    return "đã gửi tin nhắn !";
                case (int)TypeMessageEnum.Video:
                case (int)TypeMessageEnum.Videos:
                    return "đã gửi video !";
                case (int)TypeMessageEnum.Image:
                case (int)TypeMessageEnum.Images:
                    return "đã gửi hình ảnh !";
                case (int)TypeMessageEnum.Link:
                    return "đã gửi 1 liên kết !";
                default:
                    return "";
            }
           
        }

        #endregion
    }
}
