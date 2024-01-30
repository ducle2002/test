using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using Yootek.Common.Enum;
using Yootek.Organizations.Interface;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    public enum UserBillStateView
    {
        New = 1,
        Pending = 2,
        Paid = 3,
        PaidDebt = 4,
        Expired = 5
    }

    public enum UserBillStatus
    {
        Pending = 1,  // chờ thanh toán
        Paid = 2,  // đã thanh toán
        Debt = 3,  // nợ
        WaitForConfirm = 4 ,
        WaitForHandle = 5
    }

    public enum UserBillFormId
    {
        Pending_WaitForConfirm = 1,
        Paid = 2,
        Debt = 3,
    }

    public class UserBillSurcharge
    {
        public string Title { get; set; }
        public double Value { get; set; }
        public bool IsPercent { get; set; }
    }

    [Table("UserBills")]
    public class UserBill : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveBuilding, IMayHaveUrban
    {
        [CanBeNull] public string Code { get; set; }
        public DateTime? Period { get; set; }
        [StringLength(1000)] public string Title { get; set; }
        [StringLength(1000)] public string ApartmentCode { get; set; }
        public long BillConfigId { get; set; }
        public BillType BillType { get; set; }
        public double Amount { get; set; }
        public string Properties { get; set; }
        public UserBillStatus Status { get; set; }
        public int? TenantId { get; set; }
        public DateTime? DueDate { get; set; }
        public double? LastCost { get; set; }
        public decimal? DebtTotal { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public long? CitizenTempId { get; set; }
        public int? CarNumber { get; set; }
        public int? MotorbikeNumber { get; set; }
        public int? BicycleNumber { get; set; }
        public int? OtherVehicleNumber { get; set; }
        public decimal? IndexEndPeriod { get; set; }
        public decimal? IndexHeadPeriod { get; set; }
        public decimal? TotalIndex { get; set; }
        public bool? IsPaymentPending { get; set; }
        public bool? IsPrepayment { get; set; }
        public int? MonthNumber {  get; set; }
        //public int? ECarNumber {  get; set; }
        //public int? EMotorNumber {  get; set; }
        //public int? EBikeNumber {  get; set; }
    }
}