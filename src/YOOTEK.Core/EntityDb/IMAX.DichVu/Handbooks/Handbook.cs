using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("Handbooks")]
    public class Handbook : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit
    {
        public int? TenantId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Type { get; set; }
        public string FileUrl { get; set; }
        public long? OrganizationUnitId { get; set; }
    }
}
