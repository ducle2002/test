using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;


namespace Yootek.EntityDb
{
    public class SetItems : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [StringLength(256)]
        public string Name { get; set; }
        public string ListProperties { get; set; }
        public long? ItemId { get; set; }
        public string ImageUrl { get; set; }
        public int? State { get; set; }
        [StringLength(256)]
        public string Category { get; set; }
        public long? Price { get; set; }
        public int? Quantity { get; set; }
        public int? TypeGoods { get; set; }
        public long? ObjectId { get; set; }
    }
}
