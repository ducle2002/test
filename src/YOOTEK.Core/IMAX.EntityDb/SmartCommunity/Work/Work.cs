using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("Works")]
    public class Work : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<string>? ImageUrls { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateExpected { get; set; }
        public DateTime? DateFinish { get; set; }
        public string? Note { get; set; }
        public int? Status { get; set; }
        public long WorkTypeId { get; set; }
    }
}
