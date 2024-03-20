using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.RealTime;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using AutoMapper.Internal;
using Yootek.Application.Chat.Dto;
using YOOTEK.Application.GroupOrFriendshipChat.Dto;
using Yootek.Application.RoomOrFriendships.Dto;
using Yootek.Authorization.Users;
using Yootek.Chat;
using Yootek.Chat.Dto;
using Yootek.Common.DataResult;
using Yootek.Dto.Interface;
using Yootek.Friendships;
using Yootek.Friendships.Cache;
using Yootek.Friendships.Dto;
using Yootek.Users.Dto;
using Abp.Linq.Extensions;
using Yootek.Notifications;
using Yootek.EntityDb;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Yootek.Friendships
{
    [AbpAuthorize]
    public class FriendshipAppService : YootekAppServiceBase, IFriendshipAppService
    {
        private readonly IFriendshipManager _friendshipManager;
        private readonly IOnlineClientManager _onlineClientManager;
        private readonly IChatCommunicator _chatCommunicator;
        private readonly ITenantCache _tenantCache;
        private readonly IChatFeatureChecker _chatFeatureChecker;
        private readonly IUserFriendsCache _userFriendsCache;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Friendship, long> _friendshipRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IAppNotifier _appNotifier;

        public FriendshipAppService(
            IFriendshipManager friendshipManager,
            IOnlineClientManager onlineClientManager,
            IChatCommunicator chatCommunicator,
            ITenantCache tenantCache,
            IChatFeatureChecker chatFeatureChecker,
            IUserFriendsCache userFriendsCache,
            IRepository<User, long> userRepository,
            IRepository<Friendship, long> friendshipRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IAppNotifier appNotifier
        )
        {
            _friendshipManager = friendshipManager;
            _onlineClientManager = onlineClientManager;
            _chatCommunicator = chatCommunicator;
            _tenantCache = tenantCache;
            _chatFeatureChecker = chatFeatureChecker;
            _userFriendsCache = userFriendsCache;
            _userRepository = userRepository;
            _friendshipRepository = friendshipRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _appNotifier = appNotifier;
        }

        public async Task<DataResult> GetFriendRequestingList(GetAllFriendInput input)
        {
            try
            {
                var userId = AbpSession.GetUserId();

                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query =
                        (from friendship in _friendshipRepository.GetAll()
                            where friendship.UserId == AbpSession.UserId
                                  && friendship.State == FriendshipState.Requesting &&
                                  friendship.IsOrganizationUnit != true
                            select new FriendDto
                            {
                                FriendUserId = friendship.FriendUserId,
                                FriendTenantId = friendship.FriendTenantId,
                                State = friendship.State,
                                FollowState = friendship.FollowState,
                                FriendUserName = friendship.FriendUserName,
                                FriendTenancyName = friendship.FriendTenancyName,
                                FriendImageUrl = friendship.FriendImageUrl,
                                IsSender = friendship.IsSender,
                                StateAddFriend = (int)(from fr in _friendshipRepository.GetAll()
                                    where fr.FriendUserId == AbpSession.UserId
                                    select fr.State).First(),
                                LastMessageDate = friendship.CreationTime,
                                CreationTime = friendship.CreationTime,
                            })
                        .Where(x => x.IsSender == false).AsQueryable();
                    var friends = query.PageBy(input).ToList();
                    foreach (var friend in friends)
                    {
                        friend.IsOnline = await _onlineClientManager.IsOnlineAsync(
                            new UserIdentifier(friend.FriendTenantId, friend.FriendUserId)
                        );
                    }
                    return DataResult.ResultSuccess(friends, "", query.Count());
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<DataResult> GetUserRequestingList(GetAllFriendInput input)
        {
            try
            {
                var userId = AbpSession.GetUserId();

                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query =
                        (from friendship in _friendshipRepository.GetAll()
                            where friendship.UserId == AbpSession.UserId
                                  && friendship.State == FriendshipState.Requesting &&
                                  friendship.IsOrganizationUnit != true
                            select new FriendDto
                            {
                                FriendUserId = friendship.FriendUserId,
                                FriendTenantId = friendship.FriendTenantId,
                                State = friendship.State,
                                FollowState = friendship.FollowState,
                                FriendUserName = friendship.FriendUserName,
                                FriendTenancyName = friendship.FriendTenancyName,
                                FriendImageUrl = friendship.FriendImageUrl,
                                IsSender = friendship.IsSender,
                                LastMessageDate = friendship.CreationTime
                            })
                        .Where(x => x.IsSender == true).AsQueryable();
                    var friends = query.PageBy(input).ToList();
                    foreach (var friend in friends)
                    {
                        friend.IsOnline = await _onlineClientManager.IsOnlineAsync(
                            new UserIdentifier(friend.FriendTenantId, friend.FriendUserId)
                        );
                    }

                    return DataResult.ResultSuccess(friends, "", query.Count());
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
        
        public async Task<DataResult> GetFriendFollowingList(GetAllFriendInput input)
        {
            try
            {
                var userId = AbpSession.GetUserId();

                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query =
                        (from friendship in _friendshipRepository.GetAll()
                            where friendship.UserId == AbpSession.UserId
                                  && friendship.FollowState == FollowState.Following &&
                                  friendship.IsOrganizationUnit != true
                            select new FriendDto
                            {
                                FriendUserId = friendship.FriendUserId,
                                FriendTenantId = friendship.FriendTenantId,
                                State = friendship.State,
                                FollowState = friendship.FollowState,
                                FriendUserName = friendship.FriendUserName,
                                FriendTenancyName = friendship.FriendTenancyName,
                                FriendImageUrl = friendship.FriendImageUrl,
                                IsSender = friendship.IsSender,
                                StateAddFriend = (int)(from fr in _friendshipRepository.GetAll()
                                    where fr.FriendUserId == AbpSession.UserId
                                    select fr.State).First(),
                                LastMessageDate = friendship.CreationTime,
                                CreationTime = friendship.CreationTime,
                            })
                        .Where(x => x.IsSender == false).AsQueryable();
                    var friends = query.PageBy(input).ToList();
                    foreach (var friend in friends)
                    {
                        friend.IsOnline = await _onlineClientManager.IsOnlineAsync(
                            new UserIdentifier(friend.FriendTenantId, friend.FriendUserId)
                        );
                    }
                    return DataResult.ResultSuccess(friends, "", query.Count());
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
        
        public async Task<DataResult> GetFollowedList(GetAllFriendInput input)
        {
            try
            {
                var userId = AbpSession.GetUserId();

                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var query =
                        (from friendship in _friendshipRepository.GetAll()
                            where friendship.FriendUserId == AbpSession.UserId
                                  && friendship.FollowState == FollowState.Following &&
                                  friendship.IsOrganizationUnit != true
                            select new FriendDto
                            {
                                FriendUserId = friendship.FriendUserId,
                                FriendTenantId = friendship.FriendTenantId,
                                State = friendship.State,
                                FollowState = friendship.FollowState,
                                FriendUserName = friendship.FriendUserName,
                                FriendTenancyName = friendship.FriendTenancyName,
                                FriendImageUrl = friendship.FriendImageUrl,
                                IsSender = friendship.IsSender,
                                StateAddFriend = (int)(from fr in _friendshipRepository.GetAll()
                                    where fr.FriendUserId == AbpSession.UserId
                                    select fr.State).First(),
                                LastMessageDate = friendship.CreationTime,
                                CreationTime = friendship.CreationTime,
                            })
                        .Where(x => x.IsSender == false).AsQueryable();
                    var friends = query.PageBy(input).ToList();
                    foreach (var friend in friends)
                    {
                        friend.IsOnline = await _onlineClientManager.IsOnlineAsync(
                            new UserIdentifier(friend.FriendTenantId, friend.FriendUserId)
                        );
                    }

                    return DataResult.ResultSuccess(friends, "", query.Count());
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
        
        public async Task<Friendship> ChangeFriendShip(UserIdentifier userIdentifier, UserIdentifier probableFriend, FriendshipState friendshipState, FollowState followState)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var friendship = (await _friendshipManager.GetFriendshipOrNullAsync(userIdentifier, probableFriend));
                if (friendship == null)
                {
                    throw new Exception("Friendship does not exist between " + userIdentifier + " and " + probableFriend);
                }

                friendship.State = friendshipState;
                friendship.FollowState = followState;
                await _friendshipManager.UpdateFriendshipAsync(friendship);
                return friendship;
            });
        }

        public async Task<FriendDto> CreateFriendshipRequest(CreateFriendshipRequestInput input)
        {
            try
            {
                var userIdentifier = AbpSession.ToUserIdentifier();
                var probableFriend = new UserIdentifier(input.TenantId, input.UserId);
                var friendShip = await _friendshipManager.GetFriendshipOrNullAsync(userIdentifier, probableFriend);
                if (friendShip != null)
                {
                    if (friendShip.State == FriendshipState.Requesting)
                    {
                        throw new UserFriendlyException(L("YouAlreadySentAFriendshipRequestToThisUser"));
                    }

                    if (friendShip.State != FriendshipState.None || friendShip.State != FriendshipState.Stranger)
                    {
                        throw new UserFriendlyException(L("FriendShipStateIsNotValid"));
                    }
                    else
                    {
                        var clients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
                        if (clients.Any())
                        {
                            await _chatCommunicator.SendUserStateChangeToClients(clients, probableFriend,
                                FriendshipState.Requesting);
                        }

                        var senderClients = await _onlineClientManager.GetAllByUserIdAsync(probableFriend);
                        if (senderClients.Any())
                        {
                            await _chatCommunicator.SendUserStateChangeToClients(senderClients, userIdentifier,
                                FriendshipState.Requesting);
                        }

                        var followState = FollowState.Pending;
                        if (friendShip.FollowState != null)
                        {
                            followState = (FollowState)friendShip.FollowState;
                        }
                        friendShip.IsSender = true;
                        friendShip.FollowState = followState;
                        friendShip.State = FriendshipState.Requesting;
                        await _friendshipManager.UpdateFriendshipAsync(friendShip);

                        var targetFriend = await _friendshipManager.GetFriendshipOrNullAsync(probableFriend, userIdentifier);
                        if(targetFriend == null)
                        {
                            var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());
                            var userTenancyName = user.TenantId.HasValue ? _tenantCache.Get(user.TenantId.Value).TenancyName : null;
                            var targetFriendship = new Friendship(probableFriend, userIdentifier, userTenancyName,
                            user.FullName, user.ImageUrl, FriendshipState.Requesting, FollowState.Pending);
                            await _friendshipManager.CreateFriendshipAsync(targetFriendship);

                        }
                        else
                        {
                            targetFriend.IsSender = false;
                            targetFriend.FollowState = FollowState.Pending;
                            targetFriend.State = FriendshipState.Requesting;
                            await _friendshipManager.UpdateFriendshipAsync(targetFriend);
                        }


                        await NotifierNewFriendship(targetFriend, new[] { probableFriend });
                        return ObjectMapper.Map<FriendDto>(friendShip);
                    }
                }
                else
                {
                    var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());

                    User probableFriendUser;
                    using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                    {
                        probableFriendUser = (await UserManager.FindByIdAsync(input.UserId.ToString()));
                    }

                    var friendTenancyName = probableFriend.TenantId.HasValue
                        ? _tenantCache.Get(probableFriend.TenantId.Value).TenancyName
                        : null;
                    var sourceFriendship = new Friendship(userIdentifier, probableFriend, friendTenancyName,
                        probableFriendUser.FullName, probableFriendUser.ImageUrl, FriendshipState.Requesting,
                        FollowState.Pending, true);
                    await _friendshipManager.CreateFriendshipAsync(sourceFriendship);

                    //set state is requesting when friend request is not accepted
                    var userTenancyName =
                        user.TenantId.HasValue ? _tenantCache.Get(user.TenantId.Value).TenancyName : null;
                    var targetFriendship = new Friendship(probableFriend, userIdentifier, userTenancyName,
                        user.FullName, user.ImageUrl, FriendshipState.Requesting, FollowState.Pending);
                    await _friendshipManager.CreateFriendshipAsync(targetFriendship);

                    var clients = await _onlineClientManager.GetAllByUserIdAsync(probableFriend);
                    if (clients.Any())
                    {
                        var isFriendOnline =
                            await _onlineClientManager.IsOnlineAsync(sourceFriendship.ToUserIdentifier());
                        await _chatCommunicator.SendFriendshipRequestToClient(clients, targetFriendship, false,
                            isFriendOnline);
                    }

                    var senderClients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
                    if (senderClients.Any())
                    {
                        var isFriendOnline =
                            await _onlineClientManager.IsOnlineAsync(targetFriendship.ToUserIdentifier());
                        await _chatCommunicator.SendFriendshipRequestToClient(senderClients, sourceFriendship, true,
                            isFriendOnline);
                    }

                    var sourceFriendshipRequest = ObjectMapper.Map<FriendDto>(sourceFriendship);
                    sourceFriendshipRequest.IsOnline =
                        (await _onlineClientManager.GetAllByUserIdAsync(probableFriend)).Any();
                    await NotifierNewFriendship(targetFriendship, new[] { probableFriend });
                    return sourceFriendshipRequest;
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<FriendDto> CreateFriendshipRequestByUserName(CreateFriendshipRequestByUserNameInput input)
        {
            var probableFriend = await GetUserIdentifier(input.TenancyName, input.UserName);
            return await CreateFriendshipRequest(new CreateFriendshipRequestInput
            {
                TenantId = probableFriend.TenantId,
                UserId = probableFriend.UserId
            });
        }

        public async Task<FriendDto> BlockUser(BlockUserInput input)
        {
            try
            {
                var userIdentifier = AbpSession.ToUserIdentifier();
                var probableFriend = new UserIdentifier(input.TenantId, input.UserId);
                var friendShip = await _friendshipManager.GetFriendshipOrNullAsync(userIdentifier, probableFriend);
                if (friendShip != null)
                {
                    if (friendShip.State == FriendshipState.Blocked)
                        throw new UserFriendlyException(L("YouAlreadyBlockedThisUser"));
                    else
                    {
                        var clients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
                        if (clients.Any())
                        {
                            await _chatCommunicator.SendUserStateChangeToClients(clients, probableFriend,
                                FriendshipState.Blocked);
                        }

                        var senderClients = await _onlineClientManager.GetAllByUserIdAsync(probableFriend);
                        if (senderClients.Any())
                        {
                            await _chatCommunicator.SendUserStateChangeToClients(senderClients, userIdentifier,
                                FriendshipState.Blocked);
                        }
                        var followState = FollowState.Pending;
                        if (friendShip.FollowState != null)
                        {
                            followState = (FollowState)friendShip.FollowState;
                        }
                        await ChangeFriendShip(userIdentifier, probableFriend, FriendshipState.Blocked, followState);

                        return friendShip.MapTo<FriendDto>();
                    }
                }
                else
                {
                    var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());

                    User probableFriendUser;
                    using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                    {
                        probableFriendUser = (await UserManager.FindByIdAsync(input.UserId.ToString()));
                    }

                    var friendTenancyName = probableFriend.TenantId.HasValue
                        ? _tenantCache.Get(probableFriend.TenantId.Value).TenancyName
                        : null;
                    var sourceFriendship = new Friendship(userIdentifier, probableFriend, friendTenancyName,
                        probableFriendUser.FullName, probableFriendUser.ImageUrl, FriendshipState.Blocked,
                        FollowState.Pending, true);
                    await _friendshipManager.CreateFriendshipAsync(sourceFriendship);

                    //set state is requesting when friend request is not accepted
                    var userTenancyName =
                        user.TenantId.HasValue ? _tenantCache.Get(user.TenantId.Value).TenancyName : null;
                    var targetFriendship = new Friendship(probableFriend, userIdentifier, userTenancyName,
                        user.FullName, user.ImageUrl, FriendshipState.Blocked, FollowState.Pending);
                    await _friendshipManager.CreateFriendshipAsync(targetFriendship);

                    var clients = await _onlineClientManager.GetAllByUserIdAsync(probableFriend);
                    if (clients.Any())
                    {
                        var isFriendOnline =
                            await _onlineClientManager.IsOnlineAsync(sourceFriendship.ToUserIdentifier());
                        await _chatCommunicator.SendFriendshipRequestToClient(clients, targetFriendship, false,
                            isFriendOnline);
                    }

                    var senderClients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
                    if (senderClients.Any())
                    {
                        var isFriendOnline =
                            await _onlineClientManager.IsOnlineAsync(targetFriendship.ToUserIdentifier());
                        await _chatCommunicator.SendFriendshipRequestToClient(senderClients, sourceFriendship, true,
                            isFriendOnline);
                    }

                    var sourceFriendshipRequest = sourceFriendship.MapTo<FriendDto>();
                    sourceFriendshipRequest.IsOnline =
                        (await _onlineClientManager.GetAllByUserIdAsync(probableFriend)).Any();

                    return sourceFriendshipRequest;
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<FriendDto> UnblockUser(UnblockUserInput input)
        {
            try
            {
                var userIdentifier = AbpSession.ToUserIdentifier();
                var probableFriend = new UserIdentifier(input.TenantId, input.UserId);
                var friendShip = await _friendshipManager.GetFriendshipOrNullAsync(userIdentifier, probableFriend);
                if (friendShip != null)
                {
                    if (friendShip.State == FriendshipState.None)
                        throw new UserFriendlyException(L("YouAlreadyUnBlockedThisUser"));
                    if (friendShip.State != FriendshipState.Blocked)
                    {
                        throw new UserFriendlyException(L("FriendShipStateIsNotValid"));
                    }
                    else
                    {
                        var clients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
                        if (clients.Any())
                        {
                            await _chatCommunicator.SendUserStateChangeToClients(clients, probableFriend,
                                FriendshipState.None);
                        }

                        var senderClients = await _onlineClientManager.GetAllByUserIdAsync(probableFriend);
                        if (senderClients.Any())
                        {
                            await _chatCommunicator.SendUserStateChangeToClients(senderClients, userIdentifier,
                                FriendshipState.None);
                        }
                        var followState = FollowState.UnFollow;
                        if (friendShip.FollowState != null)
                        {
                            followState = (FollowState)friendShip.FollowState;
                        }
                        await ChangeFriendShip(userIdentifier, probableFriend, FriendshipState.None, followState);
                        await ChangeFriendShip(probableFriend, userIdentifier, FriendshipState.None, followState);

                        return friendShip.MapTo<FriendDto>();
                    }
                }
                else
                {
                    var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());

                    User probableFriendUser;
                    using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                    {
                        probableFriendUser = (await UserManager.FindByIdAsync(input.UserId.ToString()));
                    }

                    var friendTenancyName = probableFriend.TenantId.HasValue
                        ? _tenantCache.Get(probableFriend.TenantId.Value).TenancyName
                        : null;
                    var sourceFriendship = new Friendship(userIdentifier, probableFriend, friendTenancyName,
                        probableFriendUser.FullName, probableFriendUser.ImageUrl, FriendshipState.None,
                        FollowState.UnFollow, true);
                    await _friendshipManager.CreateFriendshipAsync(sourceFriendship);

                    //set state is requesting when friend request is not accepted
                    var userTenancyName =
                        user.TenantId.HasValue ? _tenantCache.Get(user.TenantId.Value).TenancyName : null;
                    var targetFriendship = new Friendship(probableFriend, userIdentifier, userTenancyName,
                        user.FullName, user.ImageUrl, FriendshipState.None, FollowState.UnFollow);
                    await _friendshipManager.CreateFriendshipAsync(targetFriendship);

                    var clients = await _onlineClientManager.GetAllByUserIdAsync(probableFriend);
                    if (clients.Any())
                    {
                        var isFriendOnline =
                            await _onlineClientManager.IsOnlineAsync(sourceFriendship.ToUserIdentifier());
                        await _chatCommunicator.SendFriendshipRequestToClient(clients, targetFriendship, false,
                            isFriendOnline);
                    }

                    var senderClients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
                    if (senderClients.Any())
                    {
                        var isFriendOnline =
                            await _onlineClientManager.IsOnlineAsync(targetFriendship.ToUserIdentifier());
                        await _chatCommunicator.SendFriendshipRequestToClient(senderClients, sourceFriendship, true,
                            isFriendOnline);
                    }

                    var sourceFriendshipRequest = sourceFriendship.MapTo<FriendDto>();
                    sourceFriendshipRequest.IsOnline =
                        (await _onlineClientManager.GetAllByUserIdAsync(probableFriend)).Any();

                    return sourceFriendshipRequest;
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<FriendDto> CreateFollow(FollowUserInputDto input)
        {
            try
            {
                var userIdentifier = AbpSession.ToUserIdentifier();
                var probableFriend = new UserIdentifier(input.TenantId, input.UserId);
                var friendShip = await _friendshipManager.GetFriendshipOrNullAsync(userIdentifier, probableFriend);
                if (friendShip != null)
                {
                    {
                        var clients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
                        if (clients.Any())
                        {
                            await _chatCommunicator.SendUserFollowStateChangeToClients(clients, probableFriend,
                                FollowState.Following);
                        }

                        var senderClients = await _onlineClientManager.GetAllByUserIdAsync(probableFriend);
                        if (senderClients.Any())
                        {
                            await _chatCommunicator.SendUserFollowStateChangeToClients(senderClients, userIdentifier,
                                FollowState.Following);
                        }
                        await ChangeFriendShip(userIdentifier, probableFriend, friendShip.State, FollowState.Following);

                        return friendShip.MapTo<FriendDto>();
                    }
                }
                else
                {
                    var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());
                    User probableFriendUser;
                    using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                    {
                        probableFriendUser = (await UserManager.FindByIdAsync(input.UserId.ToString()));
                    }

                    var friendTenancyName = probableFriend.TenantId.HasValue
                        ? _tenantCache.Get(probableFriend.TenantId.Value).TenancyName
                        : null;
                    var sourceFriendship = new Friendship(userIdentifier, probableFriend, friendTenancyName,
                        probableFriendUser.FullName, probableFriendUser.ImageUrl, FriendshipState.None,
                        FollowState.Following, true);
                    await _friendshipManager.CreateFriendshipAsync(sourceFriendship);
                    //set state is requesting when friend request is not accepted
                    var userTenancyName =
                        user.TenantId.HasValue ? _tenantCache.Get(user.TenantId.Value).TenancyName : null;
                    var targetFriendship = new Friendship(probableFriend, userIdentifier, userTenancyName,
                        user.FullName, user.ImageUrl, FriendshipState.None, FollowState.Following);
                    await _friendshipManager.CreateFriendshipAsync(targetFriendship);
                    var clients = await _onlineClientManager.GetAllByUserIdAsync(probableFriend);
                    if (clients.Any())
                    {
                        var isFriendOnline =
                            await _onlineClientManager.IsOnlineAsync(sourceFriendship.ToUserIdentifier());
                        await _chatCommunicator.SendFriendshipRequestToClient(clients, targetFriendship, false,
                            isFriendOnline);
                    }

                    var senderClients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
                    if (senderClients.Any())
                    {
                        var isFriendOnline =
                            await _onlineClientManager.IsOnlineAsync(targetFriendship.ToUserIdentifier());
                        await _chatCommunicator.SendFriendshipRequestToClient(senderClients, sourceFriendship, true,
                            isFriendOnline);
                    }

                    var sourceFriendshipRequest = sourceFriendship.MapTo<FriendDto>();
                    sourceFriendshipRequest.IsOnline =
                        (await _onlineClientManager.GetAllByUserIdAsync(probableFriend)).Any();
                    return sourceFriendshipRequest;
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<FriendDto> UnFollowUser(UnFollowUserInputDto input)
        {
            try
            {
                var userIdentifier = AbpSession.ToUserIdentifier();
                var probableFriend = new UserIdentifier(input.TenantId, input.UserId);
                var friendShip = await _friendshipManager.GetFriendshipOrNullAsync(userIdentifier, probableFriend);
                if (friendShip != null)
                {
                    var clients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
                    if (clients.Any())
                    {
                        await _chatCommunicator.SendUserFollowStateChangeToClients(clients, probableFriend,
                            FollowState.UnFollow);
                    }

                    var senderClients = await _onlineClientManager.GetAllByUserIdAsync(probableFriend);
                    if (senderClients.Any())
                    {
                        await _chatCommunicator.SendUserFollowStateChangeToClients(senderClients, userIdentifier,
                            FollowState.UnFollow);
                    }
                    
                    await ChangeFriendShip(userIdentifier, probableFriend, friendShip.State, FollowState.UnFollow);
                    return friendShip.MapTo<FriendDto>();
                }
                else
                {
                    var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());

                    User probableFriendUser;
                    using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                    {
                        probableFriendUser = (await UserManager.FindByIdAsync(input.UserId.ToString()));
                    }

                    var friendTenancyName = probableFriend.TenantId.HasValue
                        ? _tenantCache.Get(probableFriend.TenantId.Value).TenancyName
                        : null;
                    var sourceFriendship = new Friendship(userIdentifier, probableFriend, friendTenancyName,
                        probableFriendUser.FullName, probableFriendUser.ImageUrl, FriendshipState.None,
                        FollowState.UnFollow, true);
                    await _friendshipManager.CreateFriendshipAsync(sourceFriendship);

                    //set state is requesting when friend request is not accepted
                    var userTenancyName =
                        user.TenantId.HasValue ? _tenantCache.Get(user.TenantId.Value).TenancyName : null;
                    var targetFriendship = new Friendship(probableFriend, userIdentifier, userTenancyName,
                        user.FullName, user.ImageUrl, FriendshipState.None, FollowState.UnFollow);
                    await _friendshipManager.CreateFriendshipAsync(targetFriendship);

                    var clients = await _onlineClientManager.GetAllByUserIdAsync(probableFriend);
                    if (clients.Any())
                    {
                        var isFriendOnline =
                            await _onlineClientManager.IsOnlineAsync(sourceFriendship.ToUserIdentifier());
                        await _chatCommunicator.SendFriendshipRequestToClient(clients, targetFriendship, false,
                            isFriendOnline);
                    }

                    var senderClients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
                    if (senderClients.Any())
                    {
                        var isFriendOnline =
                            await _onlineClientManager.IsOnlineAsync(targetFriendship.ToUserIdentifier());
                        await _chatCommunicator.SendFriendshipRequestToClient(senderClients, sourceFriendship, true,
                            isFriendOnline);
                    }

                    var sourceFriendshipRequest = sourceFriendship.MapTo<FriendDto>();
                    sourceFriendshipRequest.IsOnline =
                        (await _onlineClientManager.GetAllByUserIdAsync(probableFriend)).Any();

                    return sourceFriendshipRequest;
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task AcceptFriendshipRequest(AcceptFriendshipRequestInput input)
        {
            try
            {
                var userIdentifier = AbpSession.ToUserIdentifier();
                var probableFriend = new UserIdentifier(input.TenantId, input.UserId);
                var friendShip = await _friendshipManager.GetFriendshipOrNullAsync(userIdentifier, probableFriend);
                if (friendShip != null)
                {
                    if (friendShip.State == FriendshipState.Accepted)
                    {
                        return;
                    }

                    if (friendShip.State != FriendshipState.Requesting)
                    {
                        throw new UserFriendlyException(L("FriendShipStateIsNotValid"));
                    }
                    else
                    {
                        var clients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
                        if (clients.Any())
                        {
                            await _chatCommunicator.SendUserStateChangeToClients(clients, probableFriend,
                                FriendshipState.Accepted);
                        }

                        var senderClients = await _onlineClientManager.GetAllByUserIdAsync(probableFriend);
                        if (senderClients.Any())
                        {
                            await _chatCommunicator.SendUserStateChangeToClients(senderClients, userIdentifier,
                                FriendshipState.Accepted);
                        }

                        var follow = FollowState.Following;
                        if (friendShip.FollowState != null)
                        {
                            follow = (FollowState)friendShip.FollowState;
                        }
                        await ChangeFriendShip(userIdentifier, probableFriend, FriendshipState.Accepted, follow);
                        var friend = await ChangeFriendShip(probableFriend, userIdentifier, FriendshipState.Accepted, follow);
                        await NotifierFriendshipAccepted(friend, new[] { probableFriend } );
                    }
                }
                else
                {
                    var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());

                    User probableFriendUser;
                    using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                    {
                        probableFriendUser = (await UserManager.FindByIdAsync(input.UserId.ToString()));
                    }

                    var friendTenancyName = probableFriend.TenantId.HasValue
                        ? _tenantCache.Get(probableFriend.TenantId.Value).TenancyName
                        : null;
                    var sourceFriendship = new Friendship(userIdentifier, probableFriend, friendTenancyName,
                        probableFriendUser.FullName, probableFriendUser.ImageUrl, FriendshipState.Accepted,
                        FollowState.Following, true);
                    await _friendshipManager.CreateFriendshipAsync(sourceFriendship);

                    //set state is requesting when friend request is not accepted
                    var userTenancyName =
                        user.TenantId.HasValue ? _tenantCache.Get(user.TenantId.Value).TenancyName : null;
                    var targetFriendship = new Friendship(probableFriend, userIdentifier, userTenancyName,
                        user.FullName, user.ImageUrl, FriendshipState.Accepted, FollowState.Following);
                    await _friendshipManager.CreateFriendshipAsync(targetFriendship);

                    var clients = await _onlineClientManager.GetAllByUserIdAsync(probableFriend);
                    if (clients.Any())
                    {
                        var isFriendOnline =
                            await _onlineClientManager.IsOnlineAsync(sourceFriendship.ToUserIdentifier());
                        await _chatCommunicator.SendFriendshipRequestToClient(clients, targetFriendship, false,
                            isFriendOnline);
                    }

                    var senderClients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
                    if (senderClients.Any())
                    {
                        var isFriendOnline =
                            await _onlineClientManager.IsOnlineAsync(targetFriendship.ToUserIdentifier());
                        await _chatCommunicator.SendFriendshipRequestToClient(senderClients, sourceFriendship, true,
                            isFriendOnline);
                    }

                    var sourceFriendshipRequest = sourceFriendship.MapTo<FriendDto>();
                    sourceFriendshipRequest.IsOnline =
                        (await _onlineClientManager.GetAllByUserIdAsync(probableFriend)).Any();
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task DeleteFriendshipRequest(AcceptFriendshipRequestInput input)
        {
           try
           {
               var userIdentifier = AbpSession.ToUserIdentifier();
               var probableFriend = new UserIdentifier(input.TenantId, input.UserId);
               var friendShip = await _friendshipManager.GetFriendshipOrNullAsync(userIdentifier, probableFriend);
               if (friendShip != null)
               {
                   if (friendShip.State == FriendshipState.None)
                   {
                       return;
                   }

                   if (friendShip.State != FriendshipState.Requesting)
                   {
                       throw new UserFriendlyException(L("FriendShipStateIsNotValid"));
                   }
                   else
                   {
                       var clients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
                       if (clients.Any())
                       {
                           await _chatCommunicator.SendUserStateChangeToClients(clients, probableFriend,
                               FriendshipState.None);
                       }

                       var senderClients = await _onlineClientManager.GetAllByUserIdAsync(probableFriend);
                       if (senderClients.Any())
                       {
                           await _chatCommunicator.SendUserStateChangeToClients(senderClients, userIdentifier,
                               FriendshipState.None);
                       }
                       var follow = FollowState.UnFollow;
                       if (friendShip.FollowState != null)
                       {
                           follow = (FollowState)friendShip.FollowState;
                       }
                       await ChangeFriendShip(userIdentifier, probableFriend, FriendshipState.None, follow);
                   }
               }
               else
               {
                   var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());

                   User probableFriendUser;
                   using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                   {
                       probableFriendUser = (await UserManager.FindByIdAsync(input.UserId.ToString()));
                   }

                   var friendTenancyName = probableFriend.TenantId.HasValue
                       ? _tenantCache.Get(probableFriend.TenantId.Value).TenancyName
                       : null;
                   var sourceFriendship = new Friendship(userIdentifier, probableFriend, friendTenancyName,
                       probableFriendUser.FullName, probableFriendUser.ImageUrl, FriendshipState.None,
                       FollowState.UnFollow, true);
                   await _friendshipManager.CreateFriendshipAsync(sourceFriendship);

                   //set state is requesting when friend request is not accepted
                   var userTenancyName =
                       user.TenantId.HasValue ? _tenantCache.Get(user.TenantId.Value).TenancyName : null;
                   var targetFriendship = new Friendship(probableFriend, userIdentifier, userTenancyName,
                       user.FullName, user.ImageUrl, FriendshipState.None, FollowState.UnFollow);
                   await _friendshipManager.CreateFriendshipAsync(targetFriendship);

                   var clients = await _onlineClientManager.GetAllByUserIdAsync(probableFriend);
                   if (clients.Any())
                   {
                       var isFriendOnline =
                           await _onlineClientManager.IsOnlineAsync(sourceFriendship.ToUserIdentifier());
                       await _chatCommunicator.SendFriendshipRequestToClient(clients, targetFriendship, false,
                           isFriendOnline);
                   }

                   var senderClients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
                   if (senderClients.Any())
                   {
                       var isFriendOnline =
                           await _onlineClientManager.IsOnlineAsync(targetFriendship.ToUserIdentifier());
                       await _chatCommunicator.SendFriendshipRequestToClient(senderClients, sourceFriendship, true,
                           isFriendOnline);
                   }

                   var sourceFriendshipRequest = sourceFriendship.MapTo<FriendDto>();
                   sourceFriendshipRequest.IsOnline =
                       (await _onlineClientManager.GetAllByUserIdAsync(probableFriend)).Any();
               }
           }
           catch (Exception e)
           {
               Logger.Fatal(e.Message);
               throw;
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
                var cacheItem =
                    _userFriendsCache.GetUserFriendsCacheItemInternal(AbpSession.ToUserIdentifier(),
                        FriendshipState.Accepted);
                var friendUserIds = cacheItem.Friends.Select(x => x.FriendUserId).ToList();
                var users = await UserManager.FindUserbyKeywordAsync(friendUserIds, input.Keyword, input.TenantId,
                    input.SkipCount, input.MaxResultCount);
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
          try
          {
              var userIdentifier = AbpSession.ToUserIdentifier();
              var probableFriend = new UserIdentifier(input.TenantId, input.UserId);
              var friendShip = await _friendshipManager.GetFriendshipOrNullAsync(userIdentifier, probableFriend);
              if (friendShip != null)
              {
                  if (friendShip.State == FriendshipState.None)
                  {
                      return;
                  }

                  if (friendShip.State != FriendshipState.Accepted)
                  {
                      throw new UserFriendlyException(L("FriendShipStateIsNotValid"));
                  }
                  else
                  {
                      var clients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
                      if (clients.Any())
                      {
                          await _chatCommunicator.SendUserStateChangeToClients(clients, probableFriend,
                              FriendshipState.None);
                      }

                      var senderClients = await _onlineClientManager.GetAllByUserIdAsync(probableFriend);
                      if (senderClients.Any())
                      {
                          await _chatCommunicator.SendUserStateChangeToClients(senderClients, userIdentifier,
                              FriendshipState.None);
                      }
                      var follow = FollowState.UnFollow;
                      if (friendShip.FollowState != null)
                      {
                          follow = (FollowState)friendShip.FollowState;
                      }
                      await ChangeFriendShip(userIdentifier, probableFriend, FriendshipState.None, follow);
                      await ChangeFriendShip(probableFriend, userIdentifier, FriendshipState.None, follow);

                  }
              }
              else
              {
                  var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());

                  User probableFriendUser;
                  using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                  {
                      probableFriendUser = (await UserManager.FindByIdAsync(input.UserId.ToString()));
                  }

                  var friendTenancyName = probableFriend.TenantId.HasValue
                      ? _tenantCache.Get(probableFriend.TenantId.Value).TenancyName
                      : null;
                  var sourceFriendship = new Friendship(userIdentifier, probableFriend, friendTenancyName,
                      probableFriendUser.FullName, probableFriendUser.ImageUrl, FriendshipState.None,
                      FollowState.UnFollow, true);
                  await _friendshipManager.CreateFriendshipAsync(sourceFriendship);

                  //set state is requesting when friend request is not accepted
                  var userTenancyName =
                      user.TenantId.HasValue ? _tenantCache.Get(user.TenantId.Value).TenancyName : null;
                  var targetFriendship = new Friendship(probableFriend, userIdentifier, userTenancyName,
                      user.FullName, user.ImageUrl, FriendshipState.None, FollowState.UnFollow);
                  await _friendshipManager.CreateFriendshipAsync(targetFriendship);

                  var clients = await _onlineClientManager.GetAllByUserIdAsync(probableFriend);
                  if (clients.Any())
                  {
                      var isFriendOnline =
                          await _onlineClientManager.IsOnlineAsync(sourceFriendship.ToUserIdentifier());
                      await _chatCommunicator.SendFriendshipRequestToClient(clients, targetFriendship, false,
                          isFriendOnline);
                  }

                  var senderClients = await _onlineClientManager.GetAllByUserIdAsync(userIdentifier);
                  if (senderClients.Any())
                  {
                      var isFriendOnline =
                          await _onlineClientManager.IsOnlineAsync(targetFriendship.ToUserIdentifier());
                      await _chatCommunicator.SendFriendshipRequestToClient(senderClients, sourceFriendship, true,
                          isFriendOnline);
                  }

                  var sourceFriendshipRequest = sourceFriendship.MapTo<FriendDto>();
                  sourceFriendshipRequest.IsOnline =
                      (await _onlineClientManager.GetAllByUserIdAsync(probableFriend)).Any();
              }
          }
          catch (Exception e)
          {
              Logger.Fatal(e.Message);
              throw;
          }
        }

        private async Task NotifierNewFriendship(Friendship data, UserIdentifier[] user)
        {
            var detailUrlApp = $"yoolife://app/friend-request";
            var detailUrlWA = $"yoolife://app/friend-request";
            var message = new UserMessageNotificationDataBase(
                            AppNotificationAction.FriendRequest,
                            AppNotificationIcon.FriendShipIcon,
                            TypeAction.Detail,
                            $"{data.FriendUserName} đã gửi một lời mời kết bạn. Nhấn để xem chi tiết !",
                            detailUrlApp,
                            detailUrlWA
                            );

            await _appNotifier.SendMessageNotificationInternalAsync(
               "Yoolife kết bạn !",
                $"{data.FriendUserName} đã gửi một lời mời kết bạn. Nhấn để xem chi tiết !",
                detailUrlApp,
                detailUrlWA,
                user,
                message,
                AppType.USER
                );

        }

        private async Task NotifierFriendshipAccepted(Friendship data, UserIdentifier[] user)
        {
            var detailUrlApp = $"yoolife://app/friend-accepted";
            var detailUrlWA = $"yoolife://app/friend-accepted";
            var message = new UserMessageNotificationDataBase(
                            AppNotificationAction.FriendRequest,
                            AppNotificationIcon.FriendShipIcon,
                            TypeAction.Detail,
                            $"{data.FriendUserName} chấp nhận lời mời kết bạn của bạn !. Nhấn để xem chi tiết !",
                            detailUrlApp,
                            detailUrlWA
                            );

            await _appNotifier.SendMessageNotificationInternalAsync(
                "Yoolife kết bạn !",
                $"{data.FriendUserName} chấp nhận lời mời kết bạn của bạn !",
                detailUrlApp,
                detailUrlWA,
                user,
                message,
                AppType.USER
                );

        }
    }
}