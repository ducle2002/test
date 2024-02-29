using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YOOTEK.EntityDb
{
    [Table("onepay_merchant", Schema = "payments")]
    public class OnepayMerchant : Entity<int>
    {
        [Key][Column("id")] public override int Id { get; set; }
        [Column("createdAt")] public DateTime CreatedAt { get; set; }
        [Column("createdById")] public int? CreatedById { get; set; }
        [Column("updatedAt")] public DateTime UpdatedAt { get; set; }
        [Column("updatedById")] public int? UpdatedById { get; set; }
        [Column("deletedAt")] public DateTime? DeletedAt { get; set; }
        [Column("deletedById")] public int? DeletedById { get; set; }
        [Column("tenantId")] public int? TenantId { get; set; }
        [Column("buildingId")] public int? BuildingId { get; set; }
        [Column("urbanId")] public int? UrbanId { get; set; }
        [Column("name")] public string Name { get; set; }
        [Column("merchant")] public string Merchant { get; set; }
        [Column("accessCode")] public string AccessCode { get; set; }
        [Column("description")] public string Description { get; set; }
    }
}
