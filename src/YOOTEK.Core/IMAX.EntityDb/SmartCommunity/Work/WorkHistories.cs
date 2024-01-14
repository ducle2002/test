using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("WorkHistories")]
    public class WorkHistory : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long RecipientId { get; set; }
        public long WorkId { get; set; }
        public long AssignerId { get; set; }
        public string Note { get; set; }
        public List<string> ImageUrls { get; set; }
        public DateTime ReadTime { get; set; }
    }
}
