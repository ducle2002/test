using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using IMAX.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static IMAX.IMAXServiceBase;

namespace IMAX.App.ServiceHttpClient.Dto.Imax.Business
{
    #region InvestmentPackage
    public class InvestmentPackageDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        [MaxLength(256)]
        public string Name { get; set; }
        public string? Description { get; set; }
        public long ProviderId { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public List<string>? VideoUrlList { get; set; }
        public InvestmentPackageStatus Status { get; set; }
        [MaxLength(512)]
        public string? Address { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public int TotalInvestmentTerm { get; set; } // số tháng đầu tư
        public long PricePerShare { get; set; } // giá mỗi suất riêng lẻ
        public int TotalNumberShares { get; set; } // tổng số suất
        public int NumberSharesSold { get; set; } // số suất đã bán
        public long PackagePrice { get; set; } // giá combo gói
        public string? Properties { get; set; } // các thông tin khác
        public int CountRate { get; set; }
        public double RatePoint { get; set; }
        public int Type { get; set; }
        public long ViewCount { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime CreationTime { get; set; }
    }
    public enum InvestmentPackageStatus
    {
        // OPEN = 1,
        INVESTING = 2,
        CLOSED = 3,
        // EXPIRED = 4,
    }
    public class GetAllInvestmentPackagesDto : CommonInputDto
    {
        public int? Type { get; set; }
        public long? ProviderId { get; set; }
        public InvestmentPackageOrderBy? OrderBy { get; set; }
        public InvestmentPackageFormId? FormId { get; set; }
        public int? TenantId { get; set; }
        public int? Rating { get; set; }
        public long? MinPrice { get; set; }
        public long? MaxPrice { get; set; }
        public List<string>? ProvinceCodes { get; set; }
    }
    public enum InvestmentPackageOrderBy
    {
        [FieldName("Name")]
        NAME = 1,
        [FieldName("TotalInvestmentTerm")]
        TOTAL_INVESTMENT_TERM = 2,
        [FieldName("PackagePrice")]
        PACKAGE_PRICE = 3,
        [FieldName("CountRate")]
        COUNT_RATE = 4,
        [FieldName("RatePoint")]
        RATE_POINT = 5,
        [FieldName("Id")]
        ID = 6,
    }

    public enum InvestmentPackageFormId
    {

    }
    public class GetInvestmentPackageByIdDto : Entity<long>
    {
    }

    public class CreateInvestmentPackageDto
    {
        [MaxLength(256)]
        public string Name { get; set; }
        public string? Description { get; set; }
        public long ProviderId { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public List<string>? VideoUrlList { get; set; }
        [MaxLength(512)]
        public string? Address { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public int TotalInvestmentTerm { get; set; } // số tháng đầu tư
        public long PricePerShare { get; set; } // giá mỗi suất riêng lẻ
        public int TotalNumberShares { get; set; } // tổng số suất
        public int NumberSharesSold { get; set; } // số suất đã bán
        public long PackagePrice { get; set; } // giá combo gói
        public string? Properties { get; set; } // các thông tin khác
        public int Type { get; set; } // giống type của provider
    }

    public class UpdateInvestmentPackageDto : EntityDto<long>
    {
        [MaxLength(256)]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public List<string>? VideoUrlList { get; set; }
        [MaxLength(512)]
        public string? Address { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public int? TotalInvestmentTerm { get; set; } // số tháng đầu tư
        public long? PricePerShare { get; set; } // giá mỗi suất riêng lẻ
        public int? TotalNumberShares { get; set; } // tổng số suất
        public int? NumberSharesSold { get; set; } // số suất đã bán
        public long? PackagePrice { get; set; } // giá combo gói
        public string? Properties { get; set; } // các thông tin khác
    }

    public enum TypeActionUpdateStateInvestmentPackage
    {
        CLOSE = 1,
        REOPEN = 2,
    }
    public class UpdateStateInvestmentPackageDto : Entity<long>
    {
        public TypeActionUpdateStateInvestmentPackage ActionType { get; set; }
    }

    public class DeleteInvestmentPackageDto : Entity<long>
    {
    }

    public class DeleteManyInvestmentPackageDto
    {
        public List<long>? Ids { get; set; }
        public long? ProviderId { get; set; }
    }
    #endregion

    #region InvestmentRegister
    public class CreateInvestmentRegisterDto
    {
        public long ProviderId { get; set; }
        public long InvestmentPackageId { get; set; }
        public InvestmentRegisterStatus Status { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? Note { get; set; }
        public InvestmentType InvestmentType { get; set; }
        public List<string>? ImageUrlList { get; set; }
    }
    public enum InvestmentRegisterStatus
    {
        PENDING_APPROVAL = 1,
        INVESTING = 2,
        COMPLETED = 3,
        CANCELLED = 4,
        CLOSED = 5,
    }
    public enum InvestmentType
    {
        EQUITY_INVESTMENT = 1,
        DIGITAL_ASSET_INVESTMENT = 2,
        REAL_ESTATE_INVESTMENT = 3,
        BOND_INVESTMENT = 4,
        PRIVATE_EQUITY = 5,
        OTHER = 6,
    }
    public class DeleteInvestmentRegisterDto : Entity<long>
    {
    }
    public class DeleteManyInvestmentRegistersDto
    {
        public List<long> Ids { get; set; }
    }
    public class GetAllInvestmentRegistersDto : CommonInputDto
    {
        public long? ProviderId { get; set; }
        public long? InvestmentPackageId { get; set; }
        public InvestmentType? InvestmentType { get; set; }
        public InvestmentRegisterFormId? FormId { get; set; }
        public InvestmentRegisterOrderBy? OrderBy { get; set; }
    }
    public enum InvestmentRegisterOrderBy
    {
        [FieldName("Id")]
        ID = 1,
        [FieldName("RegistrationDate")]
        REGISTRATION_DATE = 2,
    }
    public enum InvestmentRegisterFormId
    {
    }
    public class GetInvestmentRegisterByIdDto : EntityDto<long>
    {
    }
    public class GetListInvestmentRegisterByIdsDto
    {
        public List<long> Ids { get; set; }
    }
    public class InvestmentRegisterDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public long ProviderId { get; set; }
        public long InvestmentPackageId { get; set; }
        public long InvestorId { get; set; }
        public InvestmentRegisterStatus Status { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? Note { get; set; }
        public string? Properties { get; set; }
        public InvestmentType InvestmentType { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public DateTime CreationTime { get; set; }
        public long? CreatorUserId { get; set; }
    }
    public class UpdateInvestmentRegisterDto : EntityDto<long>
    {
        public DateTime? RegistrationDate { get; set; }
        public string? Note { get; set; }
        public string? Properties { get; set; }
        public InvestmentType? InvestmentType { get; set; }
        public List<string>? ImageUrlList { get; set; }
    }
    public enum TypeActionUpdateStateInvestmentRegister
    {
        APPROVAL = 1,
        COMPLETE = 2,
        CANCEL = 3,
        CLOSE = 4,
    }
    public class UpdateStateInvestmentRegisterDto : Entity<long>
    {
        public TypeActionUpdateStateInvestmentRegister TypeAction { get; set; }
    }
    #endregion

    #region InvestmentPayment 
    public class CreateInvestmentPaymentDto
    {
        public long ProviderId { get; set; }
        public long InvestmentPackageId { get; set; }
        public long InvestorId { get; set; }
        public InvestmentPaymentStatus Status { get; set; }
        public string? Note { get; set; }
        public PaymentMethod Method { get; set; }
        public long Amount { get; set; }
        public int NumberShare { get; set; }
        public long? InvestmentRegisterId { get; set; }
    }
    public class DeleteInvestmentPaymentDto : EntityDto<long>
    {
    }
    public class DeleteManyInvestmentPaymentDto
    {
        public List<long> Ids { get; set; }
    }
    public class GetAllInvestmentPaymentsDto : CommonInputDto
    {
        public InvestmentPaymentOrderBy? OrderBy { get; set; }
        public InvestmentPaymentFormId? FormId { get; set; }
    }
    public enum InvestmentPaymentOrderBy
    {
        [FieldName("Id")]
        ID = 1,
    }
    public enum InvestmentPaymentFormId
    {
    }
    public class GetInvestmentPaymentByIdDto : EntityDto<long>
    {
    }
    public class GetListInvestmentPaymentByIdsDto
    {
        public List<long> Ids { get; set; }
    }
    public class InvestmentPaymentDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public long ProviderId { get; set; }
        public long InvestmentPackageId { get; set; }
        public long InvestorId { get; set; }
        public InvestmentPaymentStatus Status { get; set; }
        public string? Note { get; set; }
        public string? Properties { get; set; }
        public PaymentMethod Method { get; set; }
        public long Amount { get; set; }
        public int NumberShare { get; set; }
        public long? InvestmentRegisterId { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime CreationTime { get; set; }
    }
    public class UpdateInvestmentPaymentDto : EntityDto<long>
    {
        public long? ProviderId { get; set; }
        public long? InvestmentPackageId { get; set; }
        public long? InvestorId { get; set; }
        public InvestmentPaymentStatus? Status { get; set; }
        public string? Note { get; set; }
        public string? Properties { get; set; }
        public PaymentMethod? Method { get; set; }
        public long? Amount { get; set; }
        public int? NumberShare { get; set; }
        public long? InvestmentRegisterId { get; set; }
    }
    public class UpdateStateInvestmentPaymentDto : EntityDto<long>
    {
        public TypeActionUpdateStateInvestmentPayment TypeAction { get; set; }
    }
    public enum TypeActionUpdateStateInvestmentPayment
    {
    }
    public enum InvestmentPaymentStatus
    {
        PENDING = 1,
        PAID = 2,
        FAILED = 3,
        CANCELED = 4,
    }
    public enum PaymentMethod
    {
        DIRECT = 1,
        MOMO = 2,
        VNPAY = 3,
        VIETELPAY = 4,
    }
    #endregion

    #region InvestmentItem
    public class GetAllInvestmentItemsDto : CommonInputDto
    {
    }
    public class InvestmentItemDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public long ProviderId { get; set; }
        public long CategoryId { get; set; }
        public string? Sku { get; set; }
        public List<string> ImageUrlList { get; set; }
        public List<string>? VideoUrlList { get; set; }
        public string Description { get; set; }
        public string? SizeInfo { get; set; }
        public string? LogisticInfo { get; set; }
        public EItemStatus Status { get; set; }
        public EItemCondition Condition { get; set; }
        public string? ComplaintPolicy { get; set; }
        public int? CountRate { get; set; }
        public double? RatePoint { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public int? Stock { get; set; }
        public int? Sales { get; set; }
        public List<AttributeOfItemDto>? AttributeList { get; set; }
        public List<ItemTierVariation>? TierVariationList { get; set; }
        public List<ItemModelDto>? ModelList { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public long? CreatorUserId { get; set; }
        public string? Properties { get; set; }
        public int Type { get; set; }
        public bool IsItemBooking { get; set; }
        public long ViewCount { get; set; }
        public bool? IsLike { get; set; } = false;
        public string? Address { get; set; }
    }

    public class ItemModelDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int? TenantId { get; set; }
        public long ItemId { get; set; }
        public bool IsDefault { get; set; }
        public string? Sku { get; set; }
        public int Stock { get; set; }
        public int Sales { get; set; }
        public double OriginalPrice { get; set; }
        public double CurrentPrice { get; set; }
        public string? ImageUrl { get; set; }
        public List<int> TierIndex { get; set; } = new List<int>();
        public string ItemName { get; set; }
    }
    public class AttributeOfItemDto
    {
        public long? Id { get; set; }
        public List<string>? ValueList { get; set; }
        public List<string>? UnitList { get; set; }
    }
    public enum EItemStatus  // chỉ dùng cho HttpInvestmentItemService
    {
        PENDING = 1,
        ACTIVATED = 2,   // đang hoạt động

        // VIOLATION = 3,   // vi phạm, bị tạm khóa 
        DEBOOSTED = 31,
        BANNED = 32,
        DELETED_BY_ADMIN = 33,

        DELETED = 4,  // đã xóa (có thể khôi phục)
        HIDDEN = 5,  //  trạng thái ẩn
        BLOCK = 6,  // ngừng kinh doanh
    }
    public enum EItemCondition
    {
        NEW = 1,
        USED = 2
    }
    public class ItemTierVariation
    {
        public string Name { get; set; }
        public List<string> OptionList { get; set; }
    }
    #endregion
    
    #region InvestmentPackageActivity
    public class InvestmentPackageActivityDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public long ProviderId { get; set; }
        public long InvestmentPackageId { get; set; }
        public string Name { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateExpect { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public List<string>? VideoUrlList { get; set; }
        public InvestmentPackageActivityEnum Status { get; set; }
        public string? Description { get; set; }
        public int Type { get; set; }
        public string? Properties { get; set; }
    }
    public enum InvestmentPackageActivityEnum
    {
        COMPLETE = 1,
        ONGOING = 2,
        CANCEL = 3,
    }
    public class GetAllInvestmentPackageActivitiesDto : CommonInputDto
    {
        public int? Type { get; set; }
        public long? ProviderId { get; set; }
        public long? InvestmentPackageId { get; set; }
        public DateTime? DateStartFrom { get; set; }
        public DateTime? DateStartTo { get; set; }
        public DateTime? DateExpectFrom { get; set; }
        public DateTime? DateExpectTo { get; set; }
    }
    public class  CreateInvestmentPackageActivitiesDto
    {
        public int? TenantId { get; set; }
        public long ProviderId { get; set; }
        public long InvestmentPackageId { get; set; }
        public string Name { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateExpect { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public List<string>? VideoUrlList { get; set; }
        public string? Description { get; set; }
        public int Type { get; set; }
        public string? Properties { get; set; }
    }
    
    public class UpdateInvestmentPackageActivityDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public long? ProviderId { get; set; }
        public long? InvestmentPackageId { get; set; }
        public string? Name { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateExpect { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public List<string>? VideoUrlList { get; set; }
        public string? Description { get; set; }
        public int? Type { get; set; }
        public string? Properties { get; set; }
        public InvestmentPackageActivityEnum? Status { get; set; }
    }
    public class DeleteInvestmentPackageActivityDto : Entity<long>
    {
    }

    public class DeleteManyInvestmentPackageActivityDto
    {
        public List<long>? Ids { get; set; }
        public long? ProviderId { get; set; }
    }
    #endregion
}
