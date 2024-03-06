using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using YOOTEK.EntityDb;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto
{
    public class AdminUserBillPaymentOutputDto : UserBillPayment
    {
        [NotMapped] public string FullName { get; set; }
        public long[] BillIds { get; set; }
        public List<BillPaidDto> BillList { get; set; }
        public List<BillPaidDto> BillListDebt { get; set; }
        public List<BillPaidDto> BillListPrepayment { get; set; }
        public List<BillDebt> DebtList { get; set; }
        public double? TotalPayment { get; set; }
        public string NameDeleterUser { get; set; }
        public string FullNameDeleter { get; set; }
        public string ImageAvatarUserDeleter { get; set; }

    }
    public class AdminSocialUserBillPaymentOutputDto : UserBillPayment
    {
        [NotMapped] public string FullName { get; set; }
        public long[] BillIds { get; set; }
        public List<BillPaidDto> BillList { get; set; }
        public List<BillPaidDto> BillListDebt { get; set; }
        public List<BillPaidDto> BillListPrepayment { get; set; }
        public List<BillDebt> DebtList { get; set; }
        public double? TotalPayment { get; set; }
        public string NameDeleterUser { get; set; }
        public string FullNameDeleter { get; set; }
        public string ImageAvatarUserDeleter { get; set; }
        public ThirdPartyPayment EPayment { get; set; }

    }

    [AutoMap(typeof(UserBill))]
    public class BillPaidDto : EntityDto<long>
    {
        public string Code { get; set; }
        public DateTime? Period { get; set; }
        public string Title { get; set; }
        public string ApartmentCode { get; set; }
        public BillType BillType { get; set; }
        public UserBillStatus Status { get; set; }
        public double? LastCost { get; set; }
        public decimal? IndexEndPeriod { get; set; }
        public decimal? IndexHeadPeriod { get; set; }
        public decimal? TotalIndex { get; set; }
        public double? PayAmount { get; set; }
        public double Amount { get; set; }
        public decimal? BalanceAmount { get; set; }
        public decimal? DebtTotal { get; set; }
    }

    [AutoMap(typeof(UserBill), typeof(UserBillPaymentHistory))]
    public class BillPaidInfoDto : EntityDto<long>
    {
        public string Code { get; set; }
        public DateTime? Period { get; set; }
        public string Title { get; set; }
        public string ApartmentCode { get; set; }
        public BillType BillType { get; set; }
        public UserBillStatus Status { get; set; }
        public double? LastCost { get; set; }
        public decimal? IndexEndPeriod { get; set; }
        public decimal? IndexHeadPeriod { get; set; }
        public decimal? TotalIndex { get; set; }
        public double? PayAmount { get; set; }
        public double Amount { get; set; }
        public decimal? DebtTotal { get; set; }
        public string Properties { get; set; }
        public int? TenantId { get; set; }
        public DateTime? DueDate { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public bool? IsPaymentPending { get; set; }
        public bool? IsPrepayment { get; set; }
    }

    public class BillPaymentInfo
    {
        public List<BillPaidDto> BillList { get; set; }
        public List<BillPaidDto> BillListDebt { get; set; }
        public List<BillPaidDto> BillListPrepayment { get; set; }
    }

    [AutoMap(typeof(ApartmentBalance))]
    public class BillPaymentBalanceDto
    {
        public int? TenantId { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public string CustomerName { get; set; }
        public string ApartmentCode { get; set; }
        public BillType? BillType { get; set; }
        public decimal Amount { get; set; }
        public long? CitizenTempId { get; set; }
        public long? UserBillId { get; set; }
        public EBalanceAction EBalanceAction { get; set; }

    }
}
