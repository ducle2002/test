using Abp;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.RealTime;
using Abp.UI;
using Yootek.Authorization.Users;
using Yootek.Chat.BusinessChat;
using Yootek.Friendships;
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
    public interface IBusinessChatMessageManager : IDomainService
    {
        Task SendMessageUserToProviderAsync(UserIdentifier sender, UserIdentifier receiver, long providerId, string message, string fileUrl, string receiverImageUrl, long? messageRepliedId, int typeMessage = 0, string friendName = null);
        Task SendMessageProviderToUserAsync(UserIdentifier sender, UserIdentifier receiver, long providerId, string message, string fileUrl, string receiverImageUrl, long? messageRepliedId, int typeMessage = 0, string friendName = null);
        Task DeleteMessageAsync(UserIdentifier sender, UserIdentifier receiver, Guid deviceMessageId);
    }

    public class BusinessChatMessageManager : YootekDomainServiceBase, IBusinessChatMessageManager
    {
        private readonly IRepository<BusinessChatMessage, long> _businessChatMessageRepos;
        private readonly IRepository<UserProviderFriendship, long> _userProviderFriendshipRepos;
        public readonly IBusinessChatCommunicator _businessChatCommunicator;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly UserManager _userManager;
        private readonly ITenantCache _tenantCache;
        private readonly IChatFeatureChecker _chatFeatureChecker;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IAppNotifier _appNotifier;

        public BusinessChatMessageManager(
             IBusinessChatCommunicator businessChatCommunicator,
             IOnlineClientManager onlineClientManage,
             UserManager userManager,
             ITenantCache tenantCache,
             IChatFeatureChecker chatFeatureChecker,
             IUnitOfWorkManager unitOfWorkManager,
             IAppNotifier appNotifier,
             IRepository<BusinessChatMessage, long> businessChatMessageRepos,
             IRepository<UserProviderFriendship, long> userProviderFriendshipRepos
            )
        {
            _onlineClientManager = onlineClientManage;
            _userManager = userManager;
            _tenantCache = tenantCache;
            _chatFeatureChecker = chatFeatureChecker;
            _unitOfWorkManager = unitOfWorkManager;
            _appNotifier = appNotifier;
            _businessChatMessageRepos = businessChatMessageRepos;
            _userProviderFriendshipRepos = userProviderFriendshipRepos;
            _businessChatCommunicator = businessChatCommunicator;
        }

        public async Task DeleteMessageAsync(UserIdentifier sender, UserIdentifier receiver, Guid deviceMessageId)
        {
            //CheckReceiverExists(receiver);
            using(var uow = UnitOfWorkManager.Begin())
            {
                var message = await _businessChatMessageRepos.FirstOrDefaultAsync(x => (x.SharedMessageId == deviceMessageId) && x.Side == ChatSide.Sender);
                if (message == null)
                {
                    //var messReceiver = await _businessChatMessageRepos.FirstOrDefaultAsync(x => (x.SharedMessageId == deviceMessageId) && x.Side == ChatSide.Receiver);
                    //if (messReceiver == null) return;
                    //await HandleDeleteMessageSenderAsync(sender, messReceiver);
                    return;

                }
                await HandleDeleteMessageSenderAsync(sender, message);
                await HandleDeleteMessageReceiverAsync(receiver, message);
                await uow.CompleteAsync();
            }
        }

        public async Task SendMessageUserToProviderAsync(UserIdentifier sender, UserIdentifier receiver, long providerId, string message, string fileUrl, string receiverImageUrl, long? messageRepliedId, int typeMessage = 0, string friendName = null)
        {
            using (var uow = UnitOfWorkManager.Begin())
            {
                var sharedMessageId = Guid.NewGuid();
                var friendships = await CheckBusinessFriendshipAsync(receiver, sender, providerId, friendName, receiverImageUrl);
                var sentMessage = await HandleSendToReceiverAsync(sender, receiver, providerId, message, fileUrl, receiverImageUrl, sharedMessageId, messageRepliedId, typeMessage, friendName);
                await HandleSendToSenderAsync(receiver, sender, providerId, message, fileUrl, sharedMessageId, messageRepliedId, typeMessage);
                await FireNotificationMessageToProviderAsync(sentMessage, sender, receiver, providerId, friendships.Item2, AppType.SELLER);
                await uow.CompleteAsync();
            }
          
        }

        public async Task SendMessageProviderToUserAsync(UserIdentifier sender, UserIdentifier receiver, long providerId, string message, string fileUrl, string receiverImageUrl, long? messageRepliedId, int typeMessage = 0, string friendName = null)
        {
            using (var uow = UnitOfWorkManager.Begin())
            {
                var sharedMessageId = Guid.NewGuid();
                var friendships = await CheckBusinessFriendshipAsync(receiver, sender, providerId, friendName, receiverImageUrl);

                var sentMessage = await HandleSendToReceiverAsync(sender, receiver, providerId, message, fileUrl, receiverImageUrl, sharedMessageId, messageRepliedId, typeMessage, friendName);
                await HandleSendToSenderAsync(receiver, sender, providerId, message, fileUrl, sharedMessageId, messageRepliedId, typeMessage);

                await FireNotificationMessageToUserAsync(sentMessage, receiver, providerId, friendships.Item1, AppType.USER);
                await uow.CompleteAsync();
            }
           
        }

        private async Task<Tuple<UserProviderFriendship, UserProviderFriendship>> CheckBusinessFriendshipAsync(UserIdentifier user, UserIdentifier userShop, long providerId, string providerName, string providerImageUrl)
        {
            var userFriend = new UserProviderFriendship();
            _unitOfWorkManager.WithUnitOfWork(() =>
            {
                using (CurrentUnitOfWork.SetTenantId(user.TenantId))
                {
                    userFriend = _userProviderFriendshipRepos.FirstOrDefault(x => x.UserId == user.UserId && x.FriendUserId == userShop.UserId && x.ProviderId == providerId);
                }
            });

            if (userFriend == null)
            {
                var userInfo = _userManager.GetUser(userShop);
                userFriend = new UserProviderFriendship()
                {
                    UserId = user.UserId,
                    FriendUserId = userShop.UserId,
                    FriendImageUrl = userInfo.ImageUrl,
                    FriendName = userInfo.UserName,
                    ProviderId = providerId,
                    TenantId = user.TenantId,
                    FriendTenantId = userShop.TenantId,
                    IsShop = false

                };
                SaveFriend(userFriend);
            }

            var shopFriend = new UserProviderFriendship();
            _unitOfWorkManager.WithUnitOfWork(() =>
            {
                using (CurrentUnitOfWork.SetTenantId(userShop.TenantId))
                {
                    shopFriend = _userProviderFriendshipRepos.FirstOrDefault(x => x.UserId == userShop.UserId && x.FriendUserId == user.UserId && x.ProviderId == providerId);
                }
            });

            if (shopFriend == null)
            {
                shopFriend = new UserProviderFriendship()
                {
                    UserId = userShop.UserId,
                    FriendUserId = user.UserId,
                    FriendImageUrl = providerImageUrl,
                    FriendName = providerName,
                    ProviderId = providerId,
                    TenantId = userShop.TenantId,
                    FriendTenantId = user.TenantId,
                    IsShop = true
                };

                SaveFriend(shopFriend);
            }

            return new Tuple<UserProviderFriendship, UserProviderFriendship>(userFriend, shopFriend);
        }

        public virtual long Save(BusinessChatMessage message)
        {
            return _unitOfWorkManager.WithUnitOfWork(() =>
            {
                using (CurrentUnitOfWork.SetTenantId(null))
                {
                    return _businessChatMessageRepos.InsertAndGetId(message);
                }
            });
        }

        public virtual long SaveFriend(UserProviderFriendship friendship)
        {
            return _unitOfWorkManager.WithUnitOfWork(() =>
            {
                using (CurrentUnitOfWork.SetTenantId(null))
                {
                    return _userProviderFriendshipRepos.InsertAndGetId(friendship);
                }
            });
        }

        private async Task<BusinessChatMessage> HandleSendToReceiverAsync(UserIdentifier senderIdentifier, UserIdentifier receiverIdentifier, long providerId, string message, string fileUrl, string receiverImageUrl, Guid sharedMessageId, long? messageRepliedId, int typeMessage, string friendName = null)
        {
          

            var sentMessage = new BusinessChatMessage(
                senderIdentifier,
                receiverIdentifier,
                providerId,
                ChatSide.Sender,
                message,
                ChatMessageReadState.Read,
                sharedMessageId,
                ChatMessageReadState.Unread,
                typeMessage != 0 ? typeMessage : CheckTypeMessage(message),
                messageRepliedId,
                fileUrl
            );

            Save(sentMessage);
            var messRep = new BusinessChatMessage();
            if (messageRepliedId != null)
            {
                messRep = await GetMessageAsync(messageRepliedId.Value, senderIdentifier.UserId, ChatSide.Sender);
            }
            var clients = await _onlineClientManager.GetAllByUserIdAsync(senderIdentifier);
            if (clients.Any())
            {
                await _businessChatCommunicator.SendMessageToClient(clients, sentMessage, messRep);
            }
           
            return sentMessage;
        }
        private async Task HandleSendToSenderAsync(UserIdentifier senderIdentifier, UserIdentifier receiverIdentifier, long providerId, string message, string fileUrl, Guid sharedMessageId, long? messageRepliedId, int typeMessage)
        {
           

            var sentMessage = new BusinessChatMessage(
                    senderIdentifier,
                    receiverIdentifier,
                    providerId,
                    ChatSide.Receiver,
                    message,
                    ChatMessageReadState.Unread,
                    sharedMessageId,
                    ChatMessageReadState.Read,
                    typeMessage != 0 ? typeMessage : CheckTypeMessage(message),
                    messageRepliedId,
                    fileUrl
                );

            Save(sentMessage);
            var messRep = new BusinessChatMessage();
            if (messageRepliedId != null)
            {
                messRep = await GetMessageAsync(messageRepliedId.Value, senderIdentifier.UserId, ChatSide.Receiver);
            }
            var clients = await _onlineClientManager.GetAllByUserIdAsync(senderIdentifier);
            if (clients.Any())
            {
                await _businessChatCommunicator.SendMessageToClient(clients, sentMessage, messRep);
            }

        }
        protected async Task<BusinessChatMessage> GetMessageAsync(long id, long userId, ChatSide side, int? tenantId = null)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
           {
               using (CurrentUnitOfWork.SetTenantId(tenantId))
               {
                   var mes = await _businessChatMessageRepos.FirstOrDefaultAsync(m => (m.Id == id && m.UserId == userId));
                   if (side == ChatSide.Sender)
                   {
                       return mes;
                   }
                   else if (side == ChatSide.Receiver && mes != null)
                   {
                       return await _businessChatMessageRepos.FirstOrDefaultAsync(m => (m.SharedMessageId == mes.SharedMessageId && m.UserId == userId));
                   }
                   return null;
               }
           });

        }
        public async Task HandleDeleteMessageSenderAsync(UserIdentifier sender, BusinessChatMessage message)
        {
            await _businessChatMessageRepos.DeleteAsync(message.Id);
            var clients = await _onlineClientManager.GetAllByUserIdAsync(sender);
            if (clients.Any())
            {
                await _businessChatCommunicator.SendDeleteBusinessMessageToClient(clients, message);
            }

        }
        public async Task HandleDeleteMessageReceiverAsync(UserIdentifier receiver, BusinessChatMessage message)
        {
            await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(receiver.TenantId))
                {
                    var mes = await _businessChatMessageRepos.FirstOrDefaultAsync(x => x.SharedMessageId == message.SharedMessageId && x.Side == ChatSide.Receiver);
                    if (mes != null)
                    {
                        await _businessChatMessageRepos.DeleteAsync(mes.Id);
                        var clients = await _onlineClientManager.GetAllByUserIdAsync(receiver);
                        if (clients.Any())
                        {
                            await _businessChatCommunicator.SendDeleteBusinessMessageToClient(clients, mes);
                        }
                    }
                }
            });
         
        }

        public async Task FireNotificationMessageToProviderAsync(BusinessChatMessage message, UserIdentifier user, UserIdentifier receiver, long providerId, UserProviderFriendship friend, AppType apptype)
        {
            var messageData = new UserMessageNotificationSeller(
                          AppNotificationAction.ChatMessage,
                          AppNotificationIcon.ChatMessageIcon,
                          TypeAction.Detail,
                          message.Message,
                          AppRouterLinks.AppSeller_ChatUser + "/" + providerId + '/' + user.ToUserIdentifierStringNoti(),
                          AppRouterLinks.AppSeller_ChatUser + "/" + providerId + '/' + user.ToUserIdentifierStringNoti(),
                          friend.FriendImageUrl,
                          "",
                          providerId
                          );
            await _appNotifier.SendMessageNotificationInternalSellerAsync(
                friend.FriendName + " đã gửi 1 tin nhắn !",
                message.Message,
                AppRouterLinks.AppSeller_ChatUser + "/" + providerId + '/' + user.ToUserIdentifierStringNoti(),
                AppRouterLinks.AppSeller_ChatUser + "/" + providerId + '/' + user.ToUserIdentifierStringNoti(),
                new [] { receiver },
                messageData,
                providerId,
                apptype
               );
        }

        public async Task FireNotificationMessageToUserAsync(BusinessChatMessage message, UserIdentifier user, long providerId,  UserProviderFriendship friend, AppType apptype)
        {
            var messageData = new UserMessageNotificationDataBase(
                          AppNotificationAction.ChatMessage,
                          AppNotificationIcon.ChatMessageIcon,
                          TypeAction.Detail,
                          message.Message,
                          AppRouterLinks.AppUser_ChatSeller + "/" + providerId,
                          AppRouterLinks.AppUser_ChatSeller + "/" + providerId,
                          friend.FriendImageUrl
                          );
            await _appNotifier.SendMessageNotificationInternalAsync(
                friend.FriendName + " đã gửi 1 tin nhắn !",
                message.Message,
                AppRouterLinks.AppUser_ChatSeller + "/" + providerId,
                AppRouterLinks.AppUser_ChatSeller + "/" + providerId,
                new [] { user },
                messageData,
                apptype
               );
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
    }
}
