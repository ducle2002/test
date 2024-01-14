using System;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Yootek.EntityDb
{
    public class Images : Entity<long>, IMayHaveTenant
    {
        public int? Type { get; set; }
        public string Properties { get; set; }
        public int? TenantId { get; set; }
    }
}
