using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.EntityDb
{
    public class RequestPaymentHistory : FullAuditedEntity<long>, IMayHaveTenant
    {
        public string Properties { get; set; }
        public Guid? RequestPaymentId { get; set; }
        public int? TenantId { get; set; }
    }
}
