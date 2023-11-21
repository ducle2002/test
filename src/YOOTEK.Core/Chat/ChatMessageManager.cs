using Abp;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.RealTime;
using Abp.UI;
using IMAX.Authorization.Users;
using IMAX.Friendships;
using IMAX.Friendships.Cache;
using IMAX.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IMAX.Chat
{
    public interface IChatMessageManager : IDomainService
    {
        Task SendMessageAsync(UserIdentifier sender, UserIdentifier receiver, string message,string fileUrl, string senderTenancyName, string senderUserName, string senderProfilePictureId, long? MessageRepliedId, int TypeMessage = 0);
        Task DeleteMessageAsync(UserIdentifier sender, UserIdentifier receiver, Guid deviceMessageId, long id);
        long Save(ChatMessage message);
        int GetUnreadMessageCount(UserIdentifier userIdentifier, UserIdentifier sender);
        Task<ChatMessage> FindMessageAsync(int id, long userId);

    }

    [AbpAuthorize]
    public class ChatMessageManager : IMAXDomainServiceBase, IChatMessageManager
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
            CheckReceiverExists(receiver);
            var message = await _chatMessageRepository.FirstOrDefaultAsync(x => (x.Id == id || x.SharedMessageId == deviceMessageId) && x.Side == ChatSide.Sender);
            if (message == null)
            {
                return;
            }
            await HandleDeleteMessageSenderAsync(sender, message);
            await HandleDeleteMessageReceiverAsync(receiver, message);

        }
        public async Task SendMessageAsync(UserIdentifier sender, UserIdentifier receiver, string message,string fileUrl, string senderTenancyName, string senderUserName, string senderProfilePictureId, long? messageRepliedId, int typeMessage = 0)
        {
             CheckReceiverExists(receiver);

            // _chatFeatureChecker.CheckChatFeatures(sender.TenantId, receiver.TenantId);

            var friendshipState = (await _friendshipManager.GetFriendshipOrNullAsync(sender, receiver))?.State;
            if (friendshipState == FriendshipState.Blocked)
            {
                throw new UserFriendlyException(L("UserIsBlocked"));
            }

            var sharedMessageId = Guid.NewGuid();

            await HandleSenderToReceiverAsync(sender, receiver, message,fileUrl, sharedMessageId, messageRepliedId, typeMessage);
            await HandleReceiverToSenderAsync(sender, receiver, message,fileUrl, sharedMessageId, messageRepliedId, typeMessage);
            await HandleSenderUserInfoChangeAsync(sender, receiver, senderTenancyName, senderUserName, senderProfilePictureId);
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

        private async Task HandleSenderToReceiverAsync(UserIdentifier senderIdentifier, UserIdentifier receiverIdentifier, string message,string fileUrl, Guid sharedMessageId, long? messageRepliedId, int typeMessage)
        {
            var friendshipState = (await _friendshipManager.GetFriendshipOrNullAsync(senderIdentifier, receiverIdentifier))?.State;
            if (friendshipState == null)
            {
                friendshipState = FriendshipState.Accepted;

                var receiverTenancyName = await GetTenancyNameOrNull(receiverIdentifier.TenantId);

                var receiverUser = await _userManager.GetUserAsync(receiverIdentifier);
                //await _friendshipManager.CreateFriendshipAsync(
                //    new Friendship(
                //        senderIdentifier,
                //        receiverIdentifier,
                //        receiverTenancyName,
                //        receiverUser.UserName,
                //        receiverUser.ImageUrl,
                //        friendshipState.Value)
                //);
            }

            if (friendshipState.Value == FriendshipState.Blocked)
            {
                //Do not send message if receiver banned the sender
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

        private async Task HandleReceiverToSenderAsync(UserIdentifier senderIdentifier, UserIdentifier receiverIdentifier, string message,string fileUrl, Guid sharedMessageId, long? messageRepliedId, int typeMessage)
        {
            var friendshipState = (await _friendshipManager.GetFriendshipOrNullAsync(receiverIdentifier, senderIdentifier))?.State;

            if (friendshipState == null)
            {
                var senderTenancyName = await GetTenancyNameOrNull(senderIdentifier.TenantId);

                var senderUser = await _userManager.GetUserAsync(senderIdentifier);
                //await _friendshipManager.CreateFriendshipAsync(
                //    new Friendship(
                //        receiverIdentifier,
                //        senderIdentifier,
                //        senderTenancyName,
                //        senderUser.UserName,
                //        senderUser.ImageUrl,
                //        FriendshipState.Accepted
                //    )
                //);
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
            else
            {
               await  FireNotificationMessageToUserAsync(sentMessage, receiverIdentifier);
            }


           
        }

        private async Task HandleSenderUserInfoChangeAsync(UserIdentifier sender, UserIdentifier receiver, string senderTenancyName, string senderUserName, string senderProfilePictureId)
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

        public async Task FireNotificationMessageToUserAsync(ChatMessage message, UserIdentifier user)
        {

            var friend = _userFriendsCache.GetCacheItem(user).Friends.Where(x => x.FriendUserId == message.TargetUserId && x.FriendTenantId == message.TenantId).FirstOrDefault();
            var messageDeclined = new UserMessageNotificationDataBase(
                          AppNotificationAction.ChatMessage,
                          AppNotificationIcon.ChatMessageIcon,
                           TypeAction.Detail,
                         friend.FriendUserName + " " +  NotificationMessageCheckType(message),
                         "",
                         ""
                          );
            await _appNotifier.SendUserMessageNotifyOnlyFirebaseAsync(
                "Imax chat . " + friend.FriendUserName,
                NotificationMessageCheckType(message),
                new UserIdentifier[] { user },
                messageDeclined,
                "",
                "");
        }

        public string NotificationMessageCheckType(ChatMessage mes)
        {
            if (mes.TypeMessage == null) return "";
            switch(mes.TypeMessage.Value)
            {
                case (int)TypeMessageEnum.File:
                case (int)TypeMessageEnum.Files:
                    return "Đã gửi file";
                case (int)TypeMessageEnum.Text:
                    return mes.Message;
                case (int)TypeMessageEnum.Video:
                case (int)TypeMessageEnum.Videos:
                    return "Đã gửi video";
                case (int)TypeMessageEnum.Image:
                case (int)TypeMessageEnum.Images:
                    return "Đã gửi hình ảnh";
                case (int)TypeMessageEnum.Link:
                    return "Đã gửi 1 liên kết";
                default:
                    return "";
            }
           
        }

        #endregion
    }
}
