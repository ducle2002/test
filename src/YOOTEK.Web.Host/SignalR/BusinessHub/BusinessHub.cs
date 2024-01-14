//using Abp;
//using Abp.AspNetCore.SignalR.Hubs;
//using Abp.Dependency;
//using Abp.MultiTenancy;
//using Abp.RealTime;
//using Castle.Core.Logging;
//using Yootek.Chat;
//using Yootek.Chat.BusinessChat;
//using System;
//using System.Threading.Tasks;

//namespace Yootek.Web.Host.SignalR.BusinessHub
//{
//    public class BusinessHub : OnlineClientHubBase, ITransientDependency
//    {

//        private readonly IBusinessChatMessageManager _busniessChatMessageManager;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="ChatHub"/> class.
//        /// </summary>
//      
//        public BusinessHub(
//            ITenantCache tenantCache,
//            IOnlineClientManager onlineClientManager,
//            IOnlineClientInfoProvider clientInfoProvider,
//            IBusinessChatMessageManager busniessChatMessageManager
//            )
//            : base(onlineClientManager, clientInfoProvider)
//        {
//            _busniessChatMessageManager = busniessChatMessageManager;

//            Logger = NullLogger.Instance;
//        }


//      
//        public async Task UserSendMessageProvider(UserSendMessageProviderInput input)
//        {
//            var sender = Context.ToUserIdentifier();
//            var receiver = new UserIdentifier(null, input.ProviderUserId);

//            try
//            {
//                using (AbpSession.Use(Context.GetTenantId(), Context.GetUserId()))
//                {
//                    await _busniessChatMessageManager.SendMessageBusinessAsync(sender, receiver, input.ProviderId, input.Message, input.ProviderImageUrl, input.MessageRepliedId, input.TypeMessage, input.ProviderName);
//                }
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(ex.Message);
//            }
//        }

//      
//        public async Task ProviderSendMessageUser(ProviderSendMessageUserInput input)
//        {
//            var sender = Context.ToUserIdentifier();
//            var receiver = new UserIdentifier(input.UserTenantId, input.UserId);

//            try
//            {
//                using (AbpSession.Use(Context.GetTenantId(), Context.GetUserId()))
//                {
//                    await _busniessChatMessageManager.SendMessageBusinessAsync(sender, receiver, input.ProviderId, input.Message, input.UserImageUrl, input.MessageRepliedId, input.TypeMessage);
//                }
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(ex.Message);
//            }
//        }

//      
//        public async Task<object> DeleteChatMessage(DeleteMessageBusinessInput input)
//        {
//            var sender = Context.ToUserIdentifier();
//            var receiver = new UserIdentifier(input.TenantId, input.TagertUserId);

//            try
//            {
//                using (AbpSession.Use(Context.GetTenantId(), Context.GetUserId()))
//                {
//                    await _busniessChatMessageManager.DeleteMessageAsync(sender, receiver, input.SharedMessageId);
//                    return true;
//                }
//            }
//            catch (Exception ex)
//            {
//                Logger.Warn("Could not send chat message to user: " + receiver);
//                Logger.Warn(ex.ToString(), ex);
//                throw new Exception(ex.Message);
//            }
//        }

//    }
//}
