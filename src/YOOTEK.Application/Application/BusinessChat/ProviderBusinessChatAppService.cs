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

        public ProviderBusinessChatAppService(
            IRepository<UserProviderFriendship, long> userProviderFriendshipRepos,
            IRepository<BusinessChatMessage, long> businessChatMessageRepos,
            IBusinessChatCommunicator businessChatCommunicator,
            IOnlineClientManager onlineClientManager
            )
        {
            _onlineClientManager = onlineClientManager;
            _businessChatMessageRepos = businessChatMessageRepos;
            _userProviderFriendshipRepos = userProviderFriendshipRepos;
            _businessChatCommunicator = businessChatCommunicator;
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
                             .Where(x => x.ProviderId == input.ProviderId && x.FriendUserId == input.UserId && x.TenantId == input.TenantId && !x.IsShop)
                             .FirstOrDefault();

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
                 m.ReadState == ChatMessageReadState.Unread
                 && m.TenantId == user.TenantId
                 )
             .ToListAsync();

            if (!messages.Any())
            {
            }

            foreach (var message in messages)
            {
                message.ChangeReadState(ChatMessageReadState.Read);
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
