using Abp.AutoMapper;
using IMAX.Common;
using IMAX.Common.Enum;
using IMAX.EntityDb;
using IMAX.Organizations.Interface;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace IMAX.Services.Dto
{
    public class UserBillDto
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public DateTime? Period { get; set; }
        public string Title { get; set; }
        public string ApartmentCode { get; set; }
        public BillType BillType { get; set; }
        public double Amount { get; set; }
        public UserBillStatus Status { get; set; }
        public double? LastCost { get; set; }
        public long? CitizenTempId { get; set; }
        public decimal? IndexEndPeriod { get; set; }
        public decimal? IndexHeadPeriod { get; set; }
        public decimal? TotalIndex { get; set; }
        public UserBillPaymentMethod? paymentMethod { get; set; }
        public string Properties { get; set; }
    }

    public class UserGetBillByMonthInput : CommonInputDto
    {
        public BillType? BillType { get; set; }
        public UserBillFormId? FormId { get; set; }
        public string ApartmentCode { get; set; }
    }

    public class GetAllUserBillsInputDto : CommonInputDto
    {
        public long[]? Ids { get; set; }
        public BillType? BillType { get; set; }
        public string? ApartmentCode { get; set; }

        //public UserBillStatus? Status { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public UserBillFormId? FormId { get; set; }
    }

    public class GetUserBillTemplateInput
    {
        public string ApartmentCode { get; set; }
        public DateTime Period { get; set; }

        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
    }

    public class HandlePayUserBillInputDto
    {
        public int ResultCode { get; set; }
        public long UserId { get; set; }
        public long PaymentId { get; set; }
        public int TenantId { get; set; }
        public long[] UserBillIds { get; set; }
        public long[] UserBillDebtIds { get; set; }
        public double Amount { get; set; }
        public UserBillPaymentMethod PaymentMethod { get; set; }
        public TypePayment TypePayment { get; set; }
        [CanBeNull] public string Properties { get; set; }
        public long[]? BillDebtIds { get; set; }
        public Guid? RequestPaymentId { get; set; }
        public string ApartmentCode { get; set; }
    }

    [AutoMap(typeof(MappingRequestPaymentDto))]
    public class HandlePayUserBillDirectInputDto
    {
        public string? Title { get; set; }
        public long[] UserBillIds { get; set; }
        public long[] UserBillDebtIds { get; set; }
        public double Amount { get; set; }
        public DateTime? Period { get; set; }
        public UserBillPaymentMethod Method { get; set; }
        [CanBeNull] public string ImgUrls { get; set; }
        [CanBeNull] public string FileUrls { get; set; }
        [CanBeNull] public string Description { get; set; }
        public long[]? BillDebtIds { get; set; }
        public TypePayment? TypePayment { get; set; }
        public long? OrganizationUnitId { get; set; }
    }

    [AutoMap(typeof(MappingRequestPaymentDto))]
    public class RequestPaymentInputDto
    {
        public int? TenantId { get; set; }
        public long[] UserBillIds { get; set; }
        public long[] UserBillDebtIds { get; set; }
        [CanBeNull] public string Title { get; set; }
        public UserBillPaymentMethod Method { get; set; }
        [CanBeNull] public string Properties { get; set; }
        public UserBillPaymentStatus Status { get; set; }
        public double Amount { get; set; }
        public DateTime? Period { get; set; }
        public long[]? BillDebtIds { get; set; }
        public TypePayment? TypePayment { get; set; }
        public Guid? RequestPaymentId { get; set; }
        public long? OrganizationUnitId { get; set; }
    }

    public class MappingRequestPaymentDto
    {
        public int? TenantId { get; set; }
        public long[] UserBillIds { get; set; }
        public long[] UserBillDebtIds { get; set; }
        [CanBeNull] public string Title { get; set; }
        public UserBillPaymentMethod Method { get; set; }
        [CanBeNull] public string Properties { get; set; }
        public UserBillPaymentStatus Status { get; set; }
        public double Amount { get; set; }
        public DateTime? Period { get; set; }
        public long[]? BillDebtIds { get; set; }
        public TypePayment? TypePayment { get; set; }
        public Guid? RequestPaymentId { get; set; }
        public long? OrganizationUnitId { get; set; }

        public UserBill UserBill { get; set; }

        [CanBeNull] public string ImgUrls { get; set; }
        [CanBeNull] public string FileUrls { get; set; }
        [CanBeNull] public string Description { get; set; }
    }


    public class GetPaymentHistoriesInputDto : CommonInputDto
    {
        public long? Id { get; set; }
        public long? UserId { get; set; }
        public UserBillPaymentStatus? Status { get; set; }
        public DateTime? PaymentDateFrom { get; set; }
        public DateTime? PaymentDateTo { get; set; }
    }

    public enum TypeHandlePayment
    {
        Normal = 1,
        TenantServiceBooking = 2
    }

    public class ApartmentBillGetAllDto : IMayHaveUrban, IMayHaveBuilding
    {
        public string Title { get; set; }
        public string ApartmentCode { get; set; }
        public long? BuildingId { get; set; }
        public long? UrbanId { get; set; }
        public DateTime Period { get; set; }
        public DateTime DueDate { get; set; }
        public UserBillStatus? Status { get; set; }
        public BillType BillType { get; set; }
        public decimal LastCost { get; set; }
        [CanBeNull] public string Properties { get; set; }
    }

    [AutoMap(typeof(BillConfig))]
    public class BillConfigDto : BillConfig
    {
        public List<BillConfig> ListPrivates { get; set; }
    }
}