using Abp;

namespace IMAX.Chat.Dto
{
    public class MarkAllUnreadMessagesOfUserAsReadInput
    {
        public int? TenantId { get; set; }

        public long UserId { get; set; }
        public long? SenderId { get; set; }
        public bool? IsOrganizationUnit { get; set; }

        public UserIdentifier ToUserIdentifier()
        {
            return new UserIdentifier(TenantId, UserId);
        }
    }
}
