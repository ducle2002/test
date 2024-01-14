using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Yootek.EntityDb
{
    public class ReportStore : Entity<long>, ICreationAudited, IMayHaveTenant
    {
        public int ObjectPartnerId { get; set; }
        public string? Detail { get; set; }
        public int? TypeReason { get; set; }
        public long? CreatorUserId { get; set; }
        public int? TenantId { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
