using Yootek.Common;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Chat.Dto
{
    public class GetUserChatMessagesInput: CommonInputDto
    {
        public int? TenantId { get; set; }

        [Range(1, long.MaxValue)]
        public long UserId { get; set; }
        public long? TargetUserId { get; set; }

        public long? MinMessageId { get; set; }
        public bool? IsOrganizationUnit { get; set; }
    }

    public class GetOrganizationChatMessagesInput: CommonInputDto
    {
        public int? TenantId { get; set; }

        [Range(1, long.MaxValue)]
        public long UserId { get; set; }
        public long? OrganizationUnitId { get; set; }

        public long? MinMessageId { get; set; }
        public bool? IsOrganizationUnit { get; set; }
    }
}