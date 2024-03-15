using Abp;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.Dependency;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.RealTime;
using Abp.Runtime.Session;
using Abp.UI;
using Castle.Core.Logging;
using Castle.Windsor;
using Yootek.Authorization.Roles;
using Yootek.Chat;
using Yootek.GroupChats;
using Yootek.Web.Host.SignalR;
using Yootek.Web.Host.SignalR.Chat.GroupChat;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Json;

namespace Yootek.Web.Host.Chat
{
    public class ChatHub : OnlineClientHubBase, ITransientDependency
    {
        /// <summary>
        /// Reference to the logger.
        /// </summary>
        //public ILogger Logger { get; set; }

        /// <summary>
        /// Reference to the session.
        /// </summary>
       // public IAbpSession AbpSession { get; set; }

        private readonly IChatMessageManager _chatMessageManager;
        private readonly ILocalizationManager _localizationManager;
        private readonly IGroupChatManager _groupChatManager;
        private readonly RoleManager _roleManager;
        private readonly IWindsorContainer _windsorContainer;
        private readonly ITenantCache _tenantCache;
        private readonly IOrganizationUnitChatManager _organizationUnitChatManager;
        private readonly IBusinessChatMessageManager _busniessChatMessageManager;
        private bool _isCallByRelease;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatHub"/> class.
        /// </summary>
      
        public ChatHub(
            IChatMessageManager chatMessageManager,
            ILocalizationManager localizationManager,
            IOnlineClientManager onlineClientManager,
            ITenantCache tenantCache,
            IGroupChatManager groupChatManager,
            IWindsorContainer windsorContainer,
            IOrganizationUnitChatManager organizationUnitChatManager,
            IBusinessChatMessageManager busniessChatMessageManager,
            RoleManager roleManager,
            IOnlineClientInfoProvider clientInfoProvider) : base(onlineClientManager, clientInfoProvider)
        {
            _chatMessageManager = chatMessageManager;
            _localizationManager = localizationManager;
            _groupChatManager = groupChatManager;
            _windsorContainer = windsorContainer;
            _tenantCache = tenantCache;
            Logger = NullLogger.Instance;
            AbpSession = NullAbpSession.Instance;
            _organizationUnitChatManager = organizationUnitChatManager;
            _busniessChatMessageManager = busniessChatMessageManager;
            _roleManager = roleManager;
        }

        #region Chat P2P
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

      
        public async Task<bool> SendMessage(SendChatMessageInput input)
        {
            var sender = Context.ToUserIdentifier();
            var receiver = new UserIdentifier(input.TenantId, input.UserId);

            try
            {
                await _chatMessageManager.SendMessageAsync(sender, receiver, input.Message, input.FileUrl, input.TenancyName, input.UserName, input.SenderImageUrl, input.MessageRepliedId, input.TypeMessage);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Fatal("Could not send chat message to user: " + receiver);
                Logger.Fatal(ex.ToJsonString());
                return false;
            }
        }

      
        public async Task<object> DeleteChatMessage(DeleteChatMessageInput input)
        {
            var sender = Context.ToUserIdentifier();
            var receiver = new UserIdentifier(input.TenantId, input.UserId);

            try
            {
                await _chatMessageManager.DeleteMessageAsync(sender, receiver, input.SharedMessageId, input.Id);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Fatal("Could not send chat message to user: " + receiver);
                Logger.Fatal(ex.ToJsonString());
                return false;
            }
        }

        public void Register()
        {
            Logger.Debug("A client is registered: " + Context.ConnectionId);
        }

        #endregion

        #region Chat Group
      
        public async Task JoinAllGroup()
        {
            try
            {
                var roomList = await _groupChatManager.GetAllGroupCode(AbpSession.ToUserIdentifier());
                if (roomList != null)
                {
                    foreach (var code in roomList)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, code);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal("JoinGroup error", ex);
            }
        }

      
        public async Task LeaveAllGroup()
        {
            try
            {
                var roomList = await _groupChatManager.GetAllGroupCode(AbpSession.ToUserIdentifier());
                if (roomList != null)
                {
                    foreach (var code in roomList)
                    {
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, code);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal("JoinGroup error", ex);
            }
        }

      
        public async Task<bool> SendMessageGroup(SendGroupChatMessageInput input)
        {
            var user = AbpSession.ToUserIdentifier();

            try
            {
                await _groupChatManager.SendGroupChatMessage(user, input.RoomId, input.Message, input.SenderImageUrl, input.TypeMessage);
                return true;

            }
            catch (UserFriendlyException ex)
            {
                Logger.Warn("Could not send chat message to user: " + user);
                Logger.Warn(ex.ToString(), ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Warn("Could not send chat message to user: " + user);
                Logger.Warn(ex.ToString(), ex);
                return false;

            }
        }

      
        public async Task<object> RecallMessageGroup(RecallMessageInput input)
        {
            try
            {
                var user = AbpSession.ToUserIdentifier();
                using (AbpSession.Use(Context.GetTenantId(), Context.GetUserId()))
                {
                    await _groupChatManager.RecallMessageGroup(user, input.RoomId, input.SharedMessageId, input.Id);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.ToString(), ex);
                return false;
            }


        }

      
        public async Task<bool> LeaveGroup(string roomCode)
        {
            try
            {
                var user = AbpSession.ToUserIdentifier();
                using (AbpSession.Use(Context.GetTenantId(), Context.GetUserId()))
                {
                    await _groupChatManager.LeaveGroupChat(user, roomCode);

                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.ToString(), ex);
                return false;
            }
          
        }


        #endregion

        #region Tenant chat organization unit

      
        public async Task<bool> SendMessageAdmin(SendMessageOrganizationInput input)
        {
            var sender = new UserIdentifier(Context.GetTenantId(), input.SenderId);
            if (!input.IsAdmin)
            {
                sender = Context.ToUserIdentifier();
            }
            input.TenantId = sender.TenantId;
            var receiver = new UserIdentifier(Context.GetTenantId(), input.UserId);

            try
            {
                await _organizationUnitChatManager.SendMessageOrgAsync(sender, receiver, input.Message, input.FileUrl, input.TenancyName, input.UserName, input.SenderImageUrl, input.MessageRepliedId, input.TypeMessage, input.IsAdmin);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warn("Could not send chat message to user: " + receiver);
                Logger.Warn(ex.ToString(), ex);
                return false;
            }
        }

      
        public async Task<object> DeleteChatMessageAdmin(DeleteChatMessageInput input)
        {
            var sender = Context.ToUserIdentifier();
            var receiver = new UserIdentifier(input.TenantId, input.UserId);

            try
            {
                await _organizationUnitChatManager.DeleteMessageOrgAsync(sender, receiver, input.SharedMessageId, input.Id);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warn("Could not send chat message to user: " + receiver);
                Logger.Warn(ex.ToString(), ex);
                return false;
            }
        }

        #endregion

        #region Booking tenant

        public async Task JoinGroupAdminBookingManager()
        {
            var code = "TenantBooking" + Context.GetTenantId();
            await Groups.AddToGroupAsync(Context.ConnectionId, code);
        }

        public async Task LeaveGroupAdminBookingManager()
        {
            var code = "TenantBooking" + Context.GetTenantId();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, code);
        }
        #endregion

        #region Business
        public async Task<bool> UserSendMessageProvider(UserSendMessageProviderInput input)
        {
            var sender = Context.ToUserIdentifier();
            var receiver = new UserIdentifier(input.ProviderTenantId, input.ProviderUserId);

            try
            {
                await _busniessChatMessageManager.SendMessageUserToProviderAsync(sender, receiver, input.ProviderId, input.Message, input.FileUrl, input.ProviderImageUrl, input.MessageRepliedId, input.TypeMessage, input.ProviderName);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task ProviderSendMessageUser(ProviderSendMessageUserInput input)
        {
            var sender = Context.ToUserIdentifier();
            var receiver = new UserIdentifier(input.UserTenantId, input.UserId);

            try
            {
                await _busniessChatMessageManager.SendMessageProviderToUserAsync(sender, receiver, input.ProviderId, input.Message, input.FileUrl, input.ProviderImageUrl, input.MessageRepliedId, input.TypeMessage, input.ProviderName);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

      
        public async Task<object> DeleteChatMessageBusiness(DeleteMessageBusinessInput input)
        {
            var sender = Context.ToUserIdentifier();
            var receiver = new UserIdentifier(input.TagertTenantId, input.TagertUserId);

            try
            {
                await _busniessChatMessageManager.DeleteMessageAsync(sender, receiver, input.SharedMessageId);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warn("Could not send chat message to user: " + receiver);
                Logger.Warn(ex.ToString(), ex);
                throw new Exception(ex.Message);
            }
        }

        #endregion
        protected override void Dispose(bool disposing)
        {
            if (_isCallByRelease)
            {
                return;
            }
            base.Dispose(disposing);
            if (disposing)
            {
                _isCallByRelease = true;
                _windsorContainer.Release(this);
            }
        }
    }
}
