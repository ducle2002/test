using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;

namespace Yootek.EntityDb
{
    public class Order : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [StringLength(256)]
        public string OrderCode { get; set; }
        public long? ObjectPartnerId { get; set; }
        public long? OrdererId { get; set; }
        public string Orderer { get; set; }
        public string Properties { get; set; }
        public int? Type { get; set; }
        public int? State { get; set; }
        public long? ItemId { get; set; }
        public int? Quantity { get; set; }
        public long? SetItemId { get; set; }
        public string Items { get; set; }
    }
}
