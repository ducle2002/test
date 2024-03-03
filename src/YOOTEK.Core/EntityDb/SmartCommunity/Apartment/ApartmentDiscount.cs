using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Common.Enum;
using Yootek.Organizations.Interface;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("ApartmentDiscounts")]
    public class ApartmentDiscount : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveUrban, IMayHaveBuilding
    {
        public int? TenantId { get; set; }
        [StringLength(256)]
        public string Name { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        [StringLength(256)]
        public string ApartmentCode { get; set; }
        public DateTime? StartPeriod { get; set; }
        public int NumberPeriod { get; set; }
        public DiscountType DiscountType { get; set; }
        public BillType BillType { get; set; }
        public decimal Value { get; set; }
    }

    public enum DiscountType
    {
        Fixing = 1,
        Percentage = 2,
        Adjust = 3

    }
}
