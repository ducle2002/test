using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Yootek.Friendships.Dto;
using Yootek.Users.Dto;

namespace Yootek.Friendships
{
    public interface IFriendshipAppService : IApplicationService
    {
        Task<List<UserDto>> FindUserToAddFriendByKeyword(FindUserToAddFriendInput input);
        Task<FriendDto> CreateFriendshipRequest(CreateFriendshipRequestInput input);
        Task<FriendDto> CreateFriendshipRequestByUserName(CreateFriendshipRequestByUserNameInput input);
        Task<FriendDto> BlockUser(BlockUserInput input);
        Task<FriendDto> UnblockUser(UnblockUserInput input);
        Task AcceptFriendshipRequest(AcceptFriendshipRequestInput input);
    }
}
