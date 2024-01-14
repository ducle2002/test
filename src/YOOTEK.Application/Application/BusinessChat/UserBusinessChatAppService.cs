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
    public interface IUserBusinessChatAppService : IApplicationService
    {

    }

    public class UserBusinessChatAppService: YootekAppServiceBase, IUserBusinessChatAppService
    {

        private readonly IOnlineClientManager _onlineClientManager;
        private readonly IRepository<BusinessChatMessage, long> _businessChatMessageRepos;
        private readonly IRepository<UserProviderFriendship, long> _userProviderFriendshipRepos;
        private readonly IBusinessChatCommunicator _businessChatCommunicator;

        public UserBusinessChatAppService(
            IRepository<UserProviderFriendship, long> userProviderFriendshipRepos,
            IRepository<BusinessChatMessage, long> businessChatMessageRepos,
            IOnlineClientManager onlineClientManager,
            IBusinessChatCommunicator businessChatCommunicator
            )
        { 
            _onlineClientManager = onlineClientManager;
            _businessChatMessageRepos = businessChatMessageRepos;
            _userProviderFriendshipRepos = userProviderFriendshipRepos;
            _businessChatCommunicator = businessChatCommunicator;

        }

        public async Task<DataResult> GetProviderFriendshipChats(GetProviderFriendshipInput input)
        {
            try
            {
                var userId = AbpSession.GetUserId();
                return await UnitOfWorkManager.WithUnitOfWorkAsync( async () =>
                {
                    var friends = _userProviderFriendshipRepos.GetAll()
                      .Where(x => x.UserId == userId && x.IsShop)
                      .WhereIf(!string.IsNullOrWhiteSpace(input.KeywordName), x => x.FriendName.Contains(input.KeywordName))
                      .ToList();
                    var result = new List<ProviderFriendshipDto>();

                    foreach (var item in friends)
                    {
                        var friend = new ProviderFriendshipDto();
                        friend = ObjectMapper.Map<ProviderFriendshipDto>(item);
                        friend.IsOnline = await _onlineClientManager.IsOnlineAsync( new UserIdentifier(null, item.FriendUserId));

                        friend.UnreadMessageCount = _businessChatMessageRepos.GetAll()
                         .Where(m => (m.UserId == userId && m.TargetUserId == friend.FriendUserId && m.ProviderId == item.ProviderId && m.ReadState == ChatMessageReadState.Unread))
                         .OrderByDescending(m => m.CreationTime)
                         .Take(20)
                         .Count();
                        friend.LastMessage = _businessChatMessageRepos.GetAll()
                           .Where(m => (m.UserId == userId && m.TargetUserId == friend.FriendUserId && m.ProviderId == item.ProviderId))
                           .OrderByDescending(m => m.CreationTime)
                           .FirstOrDefault();
                        friend.LastMessageDate = friend.LastMessage != null ? friend.LastMessage.CreationTime : friend.LastMessageDate;
                        result.Add(friend);
                    }

                    result = result.OrderByDescending(x => x.LastMessageDate).ToList();

                    return DataResult.ResultSuccess(result, "Get success !");
                });
            }
            catch (Exception e)
            {
                throw new UserFriendlyException("GetUserChatFriendsWithSetting exception !" + e.Message);
            }
        }

        public async Task<DataResult> GetBusinessChatMessages(GetProviderBusinessChatMessageInput input)
        {
            try
            {
                var user = AbpSession.ToUserIdentifier();
                var query = _businessChatMessageRepos.GetAll()
                        .Where(m => m.UserId == user.UserId && m.ProviderId == input.ProviderId && m.TenantId == user.TenantId)
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
                            var rep = await _businessChatMessageRepos.FirstOrDefaultAsync(x => x.Id == mes.MessageRepliedId && x.UserId == user.UserId);
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
                    m.UserId == user.UserId && m.ProviderId == input.ProviderId && m.TenantId == user.TenantId &&
                    m.ReadState == ChatMessageReadState.Unread
                    )
                .ToListAsync();

            if (!messages.Any())
            {
                return;
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
        }

    }
}
