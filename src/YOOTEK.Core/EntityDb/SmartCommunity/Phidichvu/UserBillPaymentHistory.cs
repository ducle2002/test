using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using Yootek.Common.Enum;
using Yootek.Organizations.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;

namespace Yootek.EntityDb
{
    [AutoMap(typeof(UserBill))]
    [Table("UserBillPaymentHistories")]
    public class UserBillPaymentHistory: FullAuditedEntity<long>, IMayHaveTenant, IMayHaveBuilding, IMayHaveUrban
    {
        public long UserBillId {  get; set; }  
        public long? PaymentId { get; set; }
        [StringLength(256)]
        public string Code { get; set; }
        public DateTime? Period { get; set; }
        [StringLength(256)]
        public string ApartmentCode { get; set; }
        public BillType BillType { get; set; }
        public UserBillStatus Status { get; set; }
        public double? LastCost { get; set; }
        public decimal? IndexEndPeriod { get; set; }
        public decimal? IndexHeadPeriod { get; set; }
        public decimal? TotalIndex { get; set; }
        public int? CarNumber { get; set; }
        public int? MotorbikeNumber { get; set; }
        public int? BicycleNumber { get; set; }
        public int? OtherVehicleNumber { get; set; }

        public double? CarPrice { get; set; }
        public double? MotorPrice { get; set; }
        public double? BikePrice { get; set; }
        public double? OtherVehiclePrice { get; set; }

        public string Vehicles { get; set; }
        public double? PayAmount { get; set; }
        public decimal? DebtTotal { get; set; }
        public int? TenantId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }
}
