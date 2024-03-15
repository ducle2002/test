using Yootek.Common;

namespace Yootek.Application.Chat.Dto
{
    public class GetUserChatFriendsWithSettingInput : CommonInputDto
    {
    }

    public class GetUserChatInput 
    {
        public long UserId { get; set; }
        public int? TenantId { get; set; }
    }
}
