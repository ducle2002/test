using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.EntityDb
{
    public class CityNotificationComment : FullAuditedEntity<long>, IMayHaveTenant
    {
        public string Comment { get; set; }
        public bool? IsLike { get; set; }
        public int? TenantId { get; set; }
        public long CityNotificationId { get; set; }
        public int? Type { get; set; }
        //public int? ParrentCommentId { get; set; }
    }
}
