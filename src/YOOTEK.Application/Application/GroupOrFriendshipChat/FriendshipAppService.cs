using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using Abp;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.MultiTenancy;
using Abp.RealTime;
using Abp.Runtime.Session;
using Abp.UI;
using IMAX;
using IMAX.Application.RoomOrFriendships.Dto;
using IMAX.Authorization.Users;
using IMAX.Chat;
using IMAX.Friendships;
using IMAX.Friendships.Cache;
using IMAX.Friendships.Dto;
using IMAX.Users.Dto;

namespace IMAX.Friendships
{
    [AbpAuthorize]
    public class FriendshipAppService : IMAXAppServiceBase, IFriendshipAppService
    {
        private readonly IFriendshipManager _friendshipManager;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly IChatCommunicator _chatCommunicator;
        private readonly ITenantCache _tenantCache;
        private readonly IChatFeatureChecker _chatFeatureChecker;
        private readonly IUserFriendsCache _userFriendsCache;

        public FriendshipAppService(
            IFriendshipManager friendshipManager,
            IOnlineClientManager onlineClientManager,
            IChatCommunicator chatCommunicator,
            ITenantCache tenantCache,
            IChatFeatureChecker chatFeatureChecker,
            IUserFriendsCache userFriendsCache)
        {
            _friendshipManager = friendshipManager;
            _onlineClientManager = onlineClientManager;
            _chatCommunicator = chatCommunicator;
            _tenantCache = tenantCache;
            _chatFeatureChecker = chatFeatureChecker;
            _userFriendsCache = userFriendsCache;
        }

       [System.Obsolete]
        public async Task<FriendDto> CreateFriendshipRequest(CreateFriendshipRequestInput input)
        {
            try
            {
                var userIdentifier = AbpSession.ToUserIdentifier();
                var probableFriend = new UserIdentifier(input.TenantId, input.UserId);

                // _chatFeatureChecker.CheckChatFeatures(userIdentifier.TenantId, probableFriend.TenantId);

                if (await _friendshipManager.GetFriendshipOrNullAsync(userIdentifier, probableFriend) != null)
                {
                    throw new UserFriendlyException(L("YouAlreadySentAFriendshipRequestToThisUser"));
                   
                }

                var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());

                User probableFriendUser;
                using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                {
                    probableFriendUser = (await UserManager.FindByIdAsync(input.UserId.ToString()));
                }

                var friendTenancyName = probableFriend.TenantId.HasValue ? _tenantCache.Get(probableFriend.TenantId.Value).TenancyName : null;
                var sourceFriendship = new Friendship(userIdentifier, probableFriend, friendTenancyName, probableFriendUser.FullName, probableFriendUser.ImageUrl, FriendshipState.Accepted, true);
                await _friendshipManager.CreateFriendshipAsync(sourceFriendship);

                //set state is requesting when friend request is not accepted
                var userTenancyName = user.TenantId.HasValue ? _tenantCache.Get(user.TenantId.Value).TenancyName : null;
                var targetFriendship = new Friendship(probableFriend, userIdentifier, userTenancyName, user.FullName, user.ImageUrl, FriendshipState.Requesting);
                await _friendshipManager.CreateFriendshipAsync(targetFriendship);

                var clients = await _onlineClientManager.GetAllByUserIdAsync(probableFriend);
                if (clients.Any())
                {
                    var isFriendOnline = await _onlineClientManager.IsOnlineAsync(sourceFriendship.ToUserIdentifier());
                    await _chatCommunicator.SendFriendshipRequestToClient(clients, targetFriendship, false, isFriendOnline);
                }

                var senderClients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
                if (senderClients.Any())
                {
                    var isFriendOnline = await _onlineClientManager.IsOnlineAsync(targetFriendship.ToUserIdentifier());
                    await _chatCommunicator.SendFriendshipRequestToClient(senderClients, sourceFriendship, true, isFriendOnline);
                }

                var sourceFriendshipRequest = sourceFriendship.MapTo<FriendDto>();
                var check = await _onlineClientManager.GetAllByUserIdAsync(probableFriend);
                sourceFriendshipRequest.IsOnline = check.Any();
                return sourceFriendshipRequest;
            }
            catch (Exception e)
            {
                throw new UserFriendlyException(e.Message);
            }
        }

        [System.Obsolete]
        public async Task<FriendDto> CreateFriendshipRequestByUserName(CreateFriendshipRequestByUserNameInput input)
        {
            var probableFriend = await GetUserIdentifier(input.TenancyName, input.UserName);
            return await CreateFriendshipRequest(new CreateFriendshipRequestInput
            {
                TenantId = probableFriend.TenantId,
                UserId = probableFriend.UserId
            });
        }

        public async Task BlockUser(BlockUserInput input)
        {
            var userIdentifier = AbpSession.ToUserIdentifier();
            var friendIdentifier = new UserIdentifier(input.TenantId, input.UserId);
            await _friendshipManager.BanFriendAsync(userIdentifier, friendIdentifier);

            var clients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
            if (clients.Any())
            {
                await _chatCommunicator.SendUserStateChangeToClients(clients, friendIdentifier, FriendshipState.Blocked);
            }
        }

        public async Task UnblockUser(UnblockUserInput input)
        {
            var userIdentifier = AbpSession.ToUserIdentifier();
            var friendIdentifier = new UserIdentifier(input.TenantId, input.UserId);
            await _friendshipManager.AcceptFriendshipRequestAsync(userIdentifier, friendIdentifier);

            var clients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
            if (clients.Any())
            {
                await _chatCommunicator.SendUserStateChangeToClients(clients, friendIdentifier, FriendshipState.Accepted);
            }
        }

        public async Task AcceptFriendshipRequest(AcceptFriendshipRequestInput input)
        {
            var userIdentifier = AbpSession.ToUserIdentifier();
            var friendIdentifier = new UserIdentifier(input.TenantId, input.UserId);
            await _friendshipManager.AcceptFriendshipRequestAsync(userIdentifier, friendIdentifier);

            var clients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
            if (clients.Any())
            {
                await _chatCommunicator.SendUserStateChangeToClients(clients, friendIdentifier, FriendshipState.Blocked);
            }

            var senderClients = await _onlineClientManager.GetAllByUserIdAsync(friendIdentifier);
            if (senderClients.Any())
            {
                await _chatCommunicator.SendUserStateChangeToClients(senderClients, userIdentifier, FriendshipState.Blocked);
            }
        }

        private async Task<UserIdentifier> GetUserIdentifier(string tenancyName, string userName)
        {
            int? tenantId = null;
            if (!tenancyName.Equals("."))
            {
                using (CurrentUnitOfWork.SetTenantId(null))
                {
                    var tenant = await TenantManager.FindByTenancyNameAsync(tenancyName);
                    if (tenant == null)
                    {
                        throw new UserFriendlyException("There is no such tenant !");
                    }

                    tenantId = tenant.Id;
                }
            }

            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var user = await UserManager.FindByNameOrEmailAsync(userName);
                if (user == null)
                {
                    throw new UserFriendlyException("There is no such user !");
                }

                return user.ToUserIdentifier();
            }
        }

      
        public async Task<List<UserDto>> FindUserToAddFriendByKeyword(FindUserToAddFriendInput input)
        {
            try
            {
                var data = new List<UserDto>();
                var cacheItem = _userFriendsCache.GetUserFriendsCacheItemInternal(AbpSession.ToUserIdentifier(), FriendshipState.Accepted);
                var friendUserIds = cacheItem.Friends.Select(x => x.FriendUserId).ToList();
                var users = await UserManager.FindUserbyKeywordAsync(friendUserIds ,input.Keyword, input.TenantId, input.SkipCount, input.MaxResultCount);
                if (users != null)
                {
                    data = users.MapTo<List<UserDto>>();
                }
                return data;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                return null;
            }
        }

        public async Task UnFriend(UnFriendInput input)
        {
            var user = AbpSession.ToUserIdentifier();
        }
    }
}