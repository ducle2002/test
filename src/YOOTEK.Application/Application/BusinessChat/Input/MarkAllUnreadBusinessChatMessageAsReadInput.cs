using Abp;

namespace IMAX.Application.BusinessChat.Input
{
    public class MarkAllUnreadBusinessChatMessageAsReadInput
    {
        public int? TenantId { get; set; }

        public long UserId { get; set; }
        public long? SenderId { get; set; }
        public long ProviderId { get; set; }

        public UserIdentifier ToUserIdentifier()
        {
            return new UserIdentifier(TenantId, UserId);
        }
    }
}
