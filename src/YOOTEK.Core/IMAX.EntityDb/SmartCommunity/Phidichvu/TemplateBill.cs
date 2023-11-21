using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using IMAX.Organizations.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMAX.IMAX.EntityDb.SmartCommunity.Phidichvu
{
    [Table("TemplateBills")]
    public class TemplateBill : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveUrban, IMayHaveBuilding
    {
        public int? TenantId { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        [StringLength(256)]
        public string Name { get; set; }
        public string Content { get; set; }
        public ETemplateBillType Type { get; set; }
    }

    public enum ETemplateBillType
    {
        BILL = 1,
        INVOICE = 2,
    }
}
