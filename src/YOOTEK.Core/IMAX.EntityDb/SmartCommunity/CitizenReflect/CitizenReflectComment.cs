using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using IMAX.Common.Enum;
using System.ComponentModel.DataAnnotations.Schema;
using static IMAX.Common.Enum.UserFeedbackCommentEnum;

namespace IMAX.EntityDb
{
    [Table("UserFeedbackComments")]
    public class CitizenReflectComment : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit
    {
        public long FeedbackId { get; set; }
        public string Comment { get; set; }
        public int? ReadState { get; set; }
        public int? TenantId { get; set; }
        public string FileUrl { get; set; }
        public TYPE_COMMENT_FEEDBACK? TypeComment { get; set; }
        public long? OrganizationUnitId { get; set; }
    }
}
