using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Yootek.Yootek.EntityDb.Clb.Event
{
    [Table("ClbEvents")]
    public class ClbEvent : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [StringLength(2000)]
        public string Name { get; set; }
        public string Description { get; set; }
        [StringLength(2000)]
        public string? FileUrl { get; set; }
        public List<string> AttachUrls { get; set; }
        public bool? IsAllowComment { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Location { get; set; }
        public string? Organizer { get; set; }
    }
}