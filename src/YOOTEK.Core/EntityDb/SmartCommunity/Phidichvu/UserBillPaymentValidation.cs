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
    [Table("UserBillPaymentValidations")]
    public class UserBillPaymentValidation : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveBuilding, IMayHaveUrban
    {
        [StringLength(1000)] public string Title { get; set; }
        [StringLength(1000)]
        public string UserBillIds { get; set; }
        [StringLength(1000)]
        public string UserBillDebtIds { get; set; }
        public UserBillPaymentMethod Method { get; set; }
        public double? Amount { get; set; }
        public DateTime? Period { get; set; }
        public int? TenantId { get; set; }
        public UserBillPaymentStatus Status { get; set; }
        public TypePayment? TypePayment { get; set; }
        [StringLength(1000)]
        public string ApartmentCode { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public string TransactionProperties { get; set; }
        public string ReturnUrl { get; set; }
        public EReturnState State { get; set; }
    }

    public enum EReturnState
    {
        Approved = 1,
        Reject = 2
    }
}