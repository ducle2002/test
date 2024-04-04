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
        //Task<DataResult> GetUserChatFriendsWithSettings(GetUserChatFriendsWithSettingInput input);

        //Task<DataResult> GetUserChatMessages(GetUserChatMessagesInput input);

        Task MarkAllUnreadMessagesOfUserAsRead(MarkAllUnreadMessagesOfUserAsReadInput input);

    }
}
