using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using Abp.Organizations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.EntityDb
{
    [Table("UserFeedbackHistories")]
    public class CitizenReflectHistory : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit
    {
        public long CitizenReflectId { get; set; }
        [StringLength(2000)]
        public string Name { get; set; }
        public string Data { get; set; }
        [StringLength(2000)]
        public string FileUrl { get; set; }
        public int? Type { get; set; }
        public int? TenantId { get; set; }
        public DateTime? FinishTime { get; set; }
        public int? State { get; set; }
        public bool? IsPublic { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string Phone { get; set; }
        public string NameFeeder { get; set; }
        public bool CheckVerify { get; set; }
        public int? CountUnreadComment { get; set; }
    }
}
