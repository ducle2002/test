using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Yootek.Application.Chat.Dto;
using Yootek.Chat.Dto;
using Yootek.Common.DataResult;

namespace Yootek.Chat
{
    public interface IChatAppService : IApplicationService
    {
        Task<GetUserChatFriendsWithSettingsOutput> GetUserChatFriendsWithSettings(GetUserChatFriendsWithSettingInput input);
        Task<GetUserChatFriendsWithSettingsOutput> GetFriendRequestingList();

        Task<ListResultDto<ChatMessageDto>> GetUserChatMessages(GetUserChatMessagesInput input);

        Task MarkAllUnreadMessagesOfUserAsRead(MarkAllUnreadMessagesOfUserAsReadInput input);

    }
}
