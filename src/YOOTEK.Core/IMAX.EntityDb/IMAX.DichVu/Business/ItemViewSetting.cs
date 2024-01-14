using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;

namespace Yootek.EntityDb
{
    public class ItemViewSetting : Entity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public int? Type { get; set; }
        [StringLength(2000)]
        public string QueryKey { get; set; }
        public string Properties { get; set; }
        public int? AttributeType { get; set; }

    }
}
