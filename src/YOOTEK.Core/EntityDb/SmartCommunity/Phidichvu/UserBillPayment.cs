using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using Yootek.Common.Enum;
using Yootek.Organizations.Interface;
using JetBrains.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("UserBillPayments")]
    public class UserBillPayment : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit, IMayHaveBuilding, IMayHaveUrban
    {
        [StringLength(1000)] public string Title { get; set; }
        [StringLength(1000)] public string PaymentCode { get; set; }
        [StringLength(1000)]
        public string? UserBillIds { get; set; }
        [StringLength(1000)]
        public string? UserBillDebtIds { get; set; }
        [StringLength(1000)]
        public string? UserBillPrepaymentIds { get; set; }
        [StringLength(1000)]
        [CanBeNull] public string BillDebtIds { get; set; }
        public UserBillPaymentMethod Method { get; set; }
        public string? Properties { get; set; }
        public double? Amount { get; set; }
        public DateTime? Period { get; set; }
        public int? TenantId { get; set; }
        public UserBillPaymentStatus Status { get; set; }
        [NotMapped] public UserBill[] UserBills { get; set; }
        [StringLength(1000)] public string? ImageUrl { get; set; }
        [StringLength(2000)] public string? Description { get; set; }
        [CanBeNull] public string FileUrl { get; set; }
        public TypePayment? TypePayment { get; set; }
        public long? OrganizationUnitId { get; set; }
        [StringLength(1000)]
        public string ApartmentCode { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public bool? IsRecover { get; set; }
        public string BillPaymentInfo { get; set; }
        [StringLength(256)]
        public string CustomerName { get; set; }
        public int? EPaymentId { get; set; }
    }

    public enum TypePayment
    {
        Bill = 1,
        BookingLocalService = 2,
        DebtBill = 3
    }
}