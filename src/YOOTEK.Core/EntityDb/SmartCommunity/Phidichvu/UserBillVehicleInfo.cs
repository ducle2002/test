using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("UserBillVehicleInfos")]
    public class UserBillVehicleInfo : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long UserBillId { get; set; }
        public long CitizenVehicleId { get; set; }
        public VehicleType VehicleType { get; set; }
        public double Cost { get; set; }
        public long? ParkingId { get; set; }
        public DateTime Period { get; set; }
    }
}
