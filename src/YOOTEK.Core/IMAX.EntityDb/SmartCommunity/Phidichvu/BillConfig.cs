using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Common.Enum;
using Yootek.Organizations.Interface;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    public enum BillConfigPricesType
    {
        Normal = 1,
        Level = 2,
        Percentage = 3,
        Rapport = 4,
        Parking = 5,
        ParkingLevel = 6,

        // Các loại hóa đơn khác
        OtherAdjust = 100,
        OtherApartmentAreas = 101,
        OtherApartmentMember = 102
    }

    [Table("BillConfigs")]
    public class BillConfig : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveBuilding, IMayHaveUrban
    {
        public BillType? BillType { get; set; }

        [StringLength(1000)]
        public string Title { get; set; }
        public int Level { get; set; }
        public VehicleType? VehicleType { get; set; }
        public long? ParentId { get; set; }
        public BillConfigPricesType? PricesType { get; set; }
        [StringLength(1000)]
        public string AppendToBillConfigIds { get; set; }
        public string Properties { get; set; }
        public int? TenantId { get; set; }
        public bool? IsDefault { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public bool? IsPrivate { get; set; }
        public long? ParkingId { get; set; }
        public Guid? PrivateKey { get; set; }
        [StringLength(50)]
        public string Code { get; set; }
        public long? ApartmentTypeId { get; set; }
    }
}
