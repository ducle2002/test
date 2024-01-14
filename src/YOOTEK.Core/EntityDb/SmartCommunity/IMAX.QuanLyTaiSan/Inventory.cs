using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Organizations.Interface;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("MaterialInventories")]
    public class Inventory : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveUrban, IMayHaveBuilding
    {
        public int? Amount { get; set; }
        public int? Price { get; set; }
        public int? TotalPrice { get; set; }
        public long MaterialId { get; set; }
        public int? TenantId { get; set; }
        public long InventoryId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }
}
