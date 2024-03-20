using Yootek.Common;

namespace Yootek.Application.BusinessChat.Input
{
    public class GetUserBusinessChatMessageInput : CommonInputDto
    {
        public long UserId { get; set; }
        public int? TenantId { get; set; }
        public long ProviderId { get; set; }
    }

    public class GetProviderBusinessChatMessageInput : CommonInputDto
    {
        public long ProviderId { get; set; }
        public long? ProviderUserId { get; set; }
    }

    public class GetProviderByIdInput
    {
        public long ProviderId { get; set;}
    }
}
