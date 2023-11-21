using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using IMAX.Friendships.Dto;
using IMAX.Users.Dto;

namespace IMAX.Friendships
{
    public interface IFriendshipAppService : IApplicationService
    {
        Task<List<UserDto>> FindUserToAddFriendByKeyword(FindUserToAddFriendInput input);
        Task<FriendDto> CreateFriendshipRequest(CreateFriendshipRequestInput input);
        Task<FriendDto> CreateFriendshipRequestByUserName(CreateFriendshipRequestByUserNameInput input);
        Task BlockUser(BlockUserInput input);
        Task UnblockUser(UnblockUserInput input);
        Task AcceptFriendshipRequest(AcceptFriendshipRequestInput input);
    }
}
