using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using IMAX.Application.Chat.Dto;
using IMAX.Chat.Dto;
using IMAX.Common.DataResult;

namespace IMAX.Chat
{
    public interface IChatAppService : IApplicationService
    {
        Task<GetUserChatFriendsWithSettingsOutput> GetUserChatFriendsWithSettings(GetUserChatFriendsWithSettingInput input);
        Task<GetUserChatFriendsWithSettingsOutput> GetFriendRequestingList();

        Task<ListResultDto<ChatMessageDto>> GetUserChatMessages(GetUserChatMessagesInput input);

        Task MarkAllUnreadMessagesOfUserAsRead(MarkAllUnreadMessagesOfUserAsReadInput input);

    }
}
