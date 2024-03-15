using Abp;
using Abp.Application.Services;
using Abp.Auditing;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.RealTime;
using Abp.Runtime.Session;
using Abp.UI;
using Yootek.Application.BusinessChat.Dto;
using Yootek.Application.BusinessChat.Input;
using Yootek.Authorization.Users;
using Yootek.Chat;
using Yootek.Chat.BusinessChat;
using Yootek.Common.DataResult;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Application.BusinessChat
{
    public class UserSendMessageProviderInput
    {
        public long ProviderId { get; set; }
        public long ProviderUserId { get; set; }
        public int? ProviderTenantId { get; set; }
        public string ProviderImageUrl { get; set; }
        public string ProviderName { get; set; }
        public long? MessageRepliedId { get; set; }
        public string Message { get; set; }
        public string FileUrl { get; set; }
        public int TypeMessage { get; set; }
    }

    public interface IProviderBusinessChatAppService : IApplicationService
    {
        Task<DataResult> GetUserFriendshipChats(GetUserFriendshipInput input);
    }

    public class ProviderBusinessChatAppService : YootekAppServiceBase, IProviderBusinessChatAppService
    {

        private readonly IOnlineClientManager _onlineClientManager;
        private readonly IRepository<BusinessChatMessage, long> _businessChatMessageRepos;
        private readonly IRepository<UserProviderFriendship, long> _userProviderFriendshipRepos;
        private readonly IBusinessChatCommunicator _businessChatCommunicator;
        private readonly IBusinessChatMessageManager _busniessChatMessageManager;
        private readonly IRepository<User, long> _userRepository;   

        public ProviderBusinessChatAppService(
            IRepository<UserProviderFriendship, long> userProviderFriendshipRepos,
            IRepository<BusinessChatMessage, long> businessChatMessageRepos,
            IBusinessChatCommunicator businessChatCommunicator,
            IOnlineClientManager onlineClientManager,
            IBusinessChatMessageManager busniessChatMessageManager,
            IRepository<User, long> userRepository

            )
        {
            _onlineClientManager = onlineClientManager;
            _businessChatMessageRepos = businessChatMessageRepos;
            _userProviderFriendshipRepos = userProviderFriendshipRepos;
            _businessChatCommunicator = businessChatCommunicator;
            _busniessChatMessageManager = busniessChatMessageManager;
            _userRepository = userRepository;
        }

        public async Task SendMessageBusiness(UserSendMessageProviderInput input)
        {
            var sender = AbpSession.ToUserIdentifier();
            var receiver = new UserIdentifier(input.ProviderTenantId, input.ProviderUserId);
            try
            {
                await _busniessChatMessageManager.SendMessageUserToProviderAsync(sender, receiver, input.ProviderId, input.Message, input.FileUrl, input.ProviderImageUrl, input.MessageRepliedId, input.TypeMessage, input.ProviderName);              
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<DataResult> GetUserFriendshipChats(GetUserFriendshipInput input)
        {
            try
            {
                var user = AbpSession.ToUserIdentifier();
                var friends = _userProviderFriendshipRepos.GetAll()
                             .Where(x => x.ProviderId == input.ProviderId && x.UserId == user.UserId && !x.IsShop)
                             .WhereIf(!string.IsNullOrWhiteSpace(input.KeywordName),x => x.FriendName.Contains(input.KeywordName) )
                             .ToList();
                var result = new List<ProviderFriendshipDto>();

                foreach (var item in friends)
                {
                    var friend = new ProviderFriendshipDto();
                    friend = ObjectMapper.Map<ProviderFriendshipDto>(item);
                    friend.IsOnline = await _onlineClientManager.IsOnlineAsync(
                        new UserIdentifier(item.TenantId, item.UserId)
                    );

                    friend.UnreadMessageCount = _businessChatMessageRepos.GetAll()
                     .Where(m => (m.UserId == user.UserId && m.TargetUserId == item.UserId && m.ProviderId == item.ProviderId && m.ReadState == ChatMessageReadState.Unread))
                     .OrderByDescending(m => m.CreationTime)
                     .Take(20)
                     .Count();
                    friend.LastMessage = _businessChatMessageRepos.GetAll()
                           .Where(m => (m.UserId == user.UserId && m.TargetUserId == friend.FriendUserId && m.ProviderId == friend.ProviderId))
                           .OrderByDescending(m => m.CreationTime)
                           .FirstOrDefault();
                    friend.LastMessageDate = friend.LastMessage != null ? friend.LastMessage.CreationTime : friend.LastMessageDate;
                    result.Add(friend);
                }

                result = result.OrderByDescending(x => x.LastMessageDate).ToList();

                return DataResult.ResultSuccess(result, "Get success !");

            }
            catch (Exception e)
            {
                throw new UserFriendlyException("GetUserChatFriendsWithSetting exception !" + e.Message);
            }
        }

        public async Task<DataResult> GetUserChat(GetUserChatInput input)
        {
            try
            {
                var user = AbpSession.ToUserIdentifier();
                var item = _userProviderFriendshipRepos.GetAll()
                             .Where(x => x.ProviderId == input.ProviderId && x.FriendUserId == input.UserId && !x.IsShop)
                             .FirstOrDefault();
                using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                {
                  
                    if (item == null)
                    {
                        item = await _userRepository.GetAll()
                            .Where(x => x.Id == input.UserId)
                            .Select(x => new UserProviderFriendship()
                            {
                                FriendUserId = input.UserId,
                                IsShop = false,
                                FriendImageUrl = x.ImageUrl,
                                ProviderId = input.ProviderId,
                                TenantId = user.TenantId,
                                UserId = user.UserId,
                                FriendTenantId = input.TenantId,
                                FriendName = x.FullName

                            }).FirstOrDefaultAsync();
                    }

                    if (item == null) return DataResult.ResultSuccess(null, "");
                    var friend = ObjectMapper.Map<ProviderFriendshipDto>(item);
                    friend.IsOnline = await _onlineClientManager.IsOnlineAsync(
                        new UserIdentifier(item.TenantId, item.UserId)
                    );

                    friend.UnreadMessageCount = _businessChatMessageRepos.GetAll()
                     .Where(m => (m.UserId == user.UserId && m.TargetUserId == item.UserId && m.ProviderId == item.ProviderId && m.ReadState == ChatMessageReadState.Unread))
                     .OrderByDescending(m => m.CreationTime)
                     .Take(20)
                     .Count();
                    friend.LastMessage = _businessChatMessageRepos.GetAll()
                           .Where(m => (m.UserId == user.UserId && m.TargetUserId == friend.FriendUserId && m.ProviderId == friend.ProviderId))
                           .OrderByDescending(m => m.CreationTime)
                           .FirstOrDefault();
                    friend.LastMessageDate = friend.LastMessage != null ? friend.LastMessage.CreationTime : friend.LastMessageDate;

                    return DataResult.ResultSuccess(friend, "Get success !");
                }

            }
            catch (Exception e)
            {
                throw new UserFriendlyException("GetUserChatFriendsWithSetting exception !" + e.Message);
            }
        }

        public async Task<DataResult> GetBusinessChatMessages(GetUserBusinessChatMessageInput input)
        {
            try
            {
                var userId = AbpSession.GetUserId();

                var query = _businessChatMessageRepos.GetAll()
                        .Where(m => m.UserId == userId && m.ProviderId == input.ProviderId && m.TargetUserId == input.UserId)
                        .OrderByDescending(m => m.CreationTime);
                var messages = await query
                         .PageBy(input)
                        .ToListAsync();

                messages.Reverse();
                var result = messages.MapTo<List<BusinessChatMessageDto>>();
                if (result != null)
                {
                    foreach (var mes in result)
                    {
                        if (mes.MessageRepliedId != null)
                        {
                            var rep = await _businessChatMessageRepos.FirstOrDefaultAsync(x => x.Id == mes.MessageRepliedId && x.UserId == userId);
                            if (rep != null)
                            {
                                mes.MessageReplied = rep.MapTo<BusinessChatMessageDto>();
                            }
                        }
                    }
                }

                return DataResult.ResultSuccess(result, "Get success !", query.Count());
            }
            catch (Exception e)
            {
                throw new UserFriendlyException("GetUserChatMessages exception !" + e.Message);
            }
        }

        public async Task MarkAllUnreadBusinessChatMessageAsRead(MarkAllUnreadBusinessChatMessageAsReadInput input)
        {
            var user = AbpSession.ToUserIdentifier();
            var messages = await _businessChatMessageRepos
             .GetAll()
             .Where(m =>
                 m.UserId == user.UserId &&
                 m.TargetUserId == input.UserId &&
                 m.ProviderId == input.ProviderId &&
                 (m.ReadState == ChatMessageReadState.Unread || m.ReceiverReadState == ChatMessageReadState.Unread)
                 && m.TenantId == user.TenantId
                 )
             .ToListAsync();

            if (!messages.Any())
            {
            }

            foreach (var message in messages)
            {
                message.ChangeReadState(ChatMessageReadState.Read);
                message.ChangeReceiverReadState(ChatMessageReadState.Read);
            }

            var userIdentifier = AbpSession.ToUserIdentifier();
            var friendIdentifier = input.ToUserIdentifier();

            var onlineClients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
            if (onlineClients.Any())
            {
                await _businessChatCommunicator.SendAllUnreadBusinessChatMessageAsReadToClients(onlineClients, friendIdentifier);
            }
            return;
        }

    }
}
