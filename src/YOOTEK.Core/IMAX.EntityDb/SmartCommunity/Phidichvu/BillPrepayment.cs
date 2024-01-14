using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Common.Enum;
using Yootek.Organizations.Interface;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("BillPrepayments")]
    public class BillPrepayment : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveBuilding, IMayHaveUrban
    {
        [StringLength(100)]
        public string Code { get; set; }
        public DateTime? StartPeriod { get; set; }
        public int NumberPeriod { get; set; }
        [StringLength(1000)] public string CustomerName { get; set; }
        [StringLength(1000)] public string ApartmentCode { get; set; }
        public BillType BillType { get; set; }
        public int TotalPaid { get; set; }
        public int? TenantId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public long? CitizenTempId { get; set; }
        public BillPrepaymentState State { get; set; }
        public string Vehicles { get; set; }
        [StringLength(100)]
        public string UserBillIds { get; set; }
        public decimal? TotalIndex { get; set; }
        public int? CarNumber { get; set; }
        public int? MotorbikeNumber { get; set; }
        public int? BicycleNumber { get; set; }
        public int? OtherVehicleNumber { get; set; }
    }


    public enum BillPrepaymentState
    {
        Active = 1,
        Inactive = 2,
        Expired = 3,
    }
}
