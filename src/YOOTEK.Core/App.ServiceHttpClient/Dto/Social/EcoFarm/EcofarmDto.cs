using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Yootek.Common;
using static Yootek.YootekServiceBase;


namespace Yootek.App.ServiceHttpClient.Dto.Business
{
    #region EcofarmPackage

    public class EcofarmPackageDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        [MaxLength(256)] public string Name { get; set; }
        public string? Description { get; set; }
        public long ProviderId { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public List<string>? VideoUrlList { get; set; }
        public List<string>? ImageUrlTimeline { get; set; }
        public List<string>? VideoUrlTimeline { get; set; }
        public int Status { get; set; }
        [MaxLength(512)] public string? Address { get; set; }
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

    public class GetAllEcofarmPackagesDto : CommonInputDto
    {
        public int? Type { get; set; }
        public long? ProviderId { get; set; }
        public int? OrderBy { get; set; }
        public int? FormId { get; set; }
        public int? TenantId { get; set; }
        public int? Rating { get; set; }
        public long? MinPrice { get; set; }
        public long? MaxPrice { get; set; }
        public List<string>? ProvinceCodes { get; set; }
    }

    public class GetEcofarmPackageByIdDto : Entity<long>
    {
    }

    public class CreateEcofarmPackageDto
    {
        [MaxLength(256)] public string Name { get; set; }
        public string? Description { get; set; }
        public long ProviderId { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public List<string>? VideoUrlList { get; set; }
        [MaxLength(512)] public string? Address { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public int TotalInvestmentTerm { get; set; } // số tháng đầu tư
        public long PricePerShare { get; set; } // giá mỗi suất riêng lẻ
        public int TotalNumberShares { get; set; } // tổng số suất
        public int NumberSharesSold { get; set; } // số suất đã bán
        public long PackagePrice { get; set; } // giá combo gói
        public string? Properties { get; set; } // các thông tin khác
    }

    public class UpdateEcofarmPackageDto : EntityDto<long>
    {
        [MaxLength(256)] public string? Name { get; set; }

        public string? Description { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public List<string>? VideoUrlList { get; set; }

        [MaxLength(512)] public string? Address { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public int? TotalInvestmentTerm { get; set; } // số tháng đầu tư
        public long? PricePerShare { get; set; } // giá mỗi suất riêng lẻ
        public int? TotalNumberShares { get; set; } // tổng số suất
        public int? NumberSharesSold { get; set; } // số suất đã bán
        public long? PackagePrice { get; set; } // giá combo gói
        public string? Properties { get; set; } // các thông tin khác
    }

    public class UpdateTimelineEcofarmPackageDto : EntityDto<long>
    {
        public List<string>? ImageUrlTimeline { get; set; }
        public List<string>? VideoUrlTimeline { get; set; }
    }

    public class UpdateStatusEcofarmPackageDto : EntityDto<long>
    {
        public int Status { get; set; }
    }

    public class DeleteEcofarmPackageDto : EntityDto<long>
    {
    }

    public class DeleteManyEcofarmPackageDto
    {
        public List<long>? Ids { get; set; }
        public long? ProviderId { get; set; }
    }

    #endregion

    #region EcofarmRegister

    public class CreateEcofarmRegisterDto
    {
        public long EcofarmPackageId { get; set; }
        public string? Note { get; set; }
        public int EcofarmType { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public int NumberOfShared { get; set; }
        public long TotalPrice { get; set; }
        public int PaymentMethod { get; set; }
    }

    public class DeleteEcofarmRegisterDto : EntityDto<long>
    {
    }

    public class DeleteManyEcofarmRegistersDto
    {
        public List<long> Ids { get; set; }
    }

    public class GetAllEcofarmRegistersDto : CommonInputDto
    {
        public long? ProviderId { get; set; }
        public long? EcofarmPackageId { get; set; }
        public int? EcofarmType { get; set; }
        public int? FormId { get; set; }
        public int? OrderBy { get; set; }
    }

    public class GetEcofarmRegisterByIdDto : EntityDto<long>
    {
    }

    public class GetListEcofarmRegisterByIdsDto
    {
        public List<long> Ids { get; set; }
    }

    public class EcofarmRegisterDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public long ProviderId { get; set; }
        public long EcofarmPackageId { get; set; }
        public long InvestorId { get; set; }
        public int Status { get; set; }
        public string? Note { get; set; }
        public string? Properties { get; set; }
        public int EcofarmType { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public DateTime CreationTime { get; set; }
        public long? CreatorUserId { get; set; }
        public int PaymentMethod { get; set; }
        public int PaymentStatus { get; set; }
        public int? NumberOfShared { get; set; }
        public long? TotalPrice { get; set; }
        public long? PartnerId { get; set; }
        public UserInfoDto? UserInfo { get; set; }
        public UserInfoDto? PartnerInfo { get; set; }
    }

    public class UserInfoDto : EntityDto<long>
    {
        public int? TenantId { get; set; } 
        public string Name { get; set; }
        public string SurName { get; set; }
        public string EmailAddress { get; set; }
    }
    public class UpdateEcofarmRegisterDto : EntityDto<long>
    {
        public DateTime? RegistrationDate { get; set; }
        public string? Note { get; set; }
        public string? Properties { get; set; }
        public int? EcofarmType { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public int? NumberOfShared { get; set; }
    }

    public class UpdateStatusEcofarmRegisterDto : Entity<long>
    {
        public int Status { get; set; }
    }

    #endregion

    #region EcofarmPayment

    public class CreateEcofarmPaymentDto
    {
        public long ProviderId { get; set; }
        public long EcofarmPackageId { get; set; }
        public long InvestorId { get; set; }
        public int Status { get; set; }
        public string? Note { get; set; }
        public int Method { get; set; }
        public long Amount { get; set; }
        public int NumberShare { get; set; }
        public long? EcofarmRegisterId { get; set; }
    }

    public class DeleteEcofarmPaymentDto : EntityDto<long>
    {
    }

    public class DeleteManyEcofarmPaymentDto
    {
        public List<long> Ids { get; set; }
    }

    public class GetAllEcofarmPaymentsDto : CommonInputDto
    {
        public int? OrderBy { get; set; }
        public int? FormId { get; set; }
    }

    public class GetEcofarmPaymentByIdDto : EntityDto<long>
    {
    }

    public class GetListEcofarmPaymentByIdsDto
    {
        public List<long> Ids { get; set; }
    }

    public class EcofarmPaymentDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public long ProviderId { get; set; }
        public long EcofarmPackageId { get; set; }
        public long InvestorId { get; set; }
        public int Status { get; set; }
        public string? Note { get; set; }
        public string? Properties { get; set; }
        public int Method { get; set; }
        public long Amount { get; set; }
        public int NumberShare { get; set; }
        public long? EcofarmRegisterId { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class UpdateEcofarmPaymentDto : EntityDto<long>
    {
        public long? ProviderId { get; set; }
        public long? EcofarmPackageId { get; set; }
        public long? InvestorId { get; set; }
        public int? Status { get; set; }
        public string? Note { get; set; }
        public string? Properties { get; set; }
        public int? Method { get; set; }
        public long? Amount { get; set; }
        public int? NumberShare { get; set; }
        public long? EcofarmRegisterId { get; set; }
    }

    public class UpdateStateEcofarmPaymentDto : EntityDto<long>
    {
        public int State { get; set; }
    }

    #endregion

    #region EcofarmItem

    public class GetAllEcofarmItemsByPartnerDto : CommonInputDto
    {
        public long? ProviderId { get; set; }
        public int? FormId { get; set; }
        public int? OrderBy { get; set; }
        public long? CategoryId { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public int? MinStock { get; set; }
        public int? MaxStock { get; set; }
        public int? MinSales { get; set; }
        public int? MaxSales { get; set; }
        public int? Rating { get; set; }
        public long? EcofarmPackageId { get; set; }
    }

    public class GetAllItemsByUserDto : CommonInputDto
    {
        public long? ProviderId { get; set; }
        public int? FormId { get; set; }
        public int? OrderBy { get; set; }
        public long? CategoryId { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public int? MinStock { get; set; }
        public int? MaxStock { get; set; }
        public int? MinSales { get; set; }
        public int? MaxSales { get; set; }
        public int? Rating { get; set; }
        public long? EcofarmPackageId { get; set; }
    }

    public class GetItemEcofarmByIdDto : EntityDto<long>
    {
    }

    public class CreateItemForEcoFarmDto
    {
        public long ProviderId { get; set; }
        public string Name { get; set; }
        public long CategoryId { get; set; }
        public string? Sku { get; set; }
        public List<string>? ImageUrlList { get; set; } = new();
        public List<string>? VideoUrlList { get; set; } = new();
        public string? Description { get; set; }
        public string? SizeInfo { get; set; }
        public string? LogisticInfo { get; set; }
        public List<AttributeOfItemDto>? AttributeList { get; set; } = new();
        public List<ItemTierVariation>? TierVariationList { get; set; } = new();
        public List<ModelOfCreateItemEcofarmDto>? ModelList { get; set; } = new();
        public string? Properties { get; set; }
        public long? EcofarmPackageId { get; set; }
    }

    public class ModelOfCreateItemEcofarmDto
    {
        public string? Name { get; set; }
        public int? TenantId { get; set; }
        public long ItemId { get; set; }
        public bool? IsDefault { get; set; } = false;
        public string Sku { get; set; }
        public int Stock { get; set; }
        public double OriginalPrice { get; set; }
        public double CurrentPrice { get; set; }
        public string ImageUrl { get; set; }
        public List<int> TierIndex { get; set; }
    }

    public class DeleteItemForEcoFarmDto : EntityDto<long>
    {
    }

    public class UpdateItemForEcoFarmDto : EntityDto<long>
    {
        public string? Name { get; set; }
        public string? Sku { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public List<string>? VideoUrlList { get; set; }
        public string? Description { get; set; }
        public string? SizeInfo { get; set; }
        public string? LogisticInfo { get; set; }
        public string? Properties { get; set; }
        public long? EcofarmPackageId { get; set; }
        public double? RatePoint { get; set; }
        public int? CountRate { get; set; }
    }

    public class UpdateItemStatusEcoFarmDto : EntityDto<long>
    {
        public int Status { get; set; }
    }

    public class EcofarmItemDto : EntityDto<long>
    {
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
        public long ViewCount { get; set; }
        public bool? IsLike { get; set; } = false;
        public string? Address { get; set; }
        public long? BusinessType { get; set; }
        public long? EcofarmPackageId { get; set; }
        public string? EcofarmPackageName { get; set; }
    }

    public class ItemModelDto : EntityDto<long>
    {
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
        public List<int> TierIndex { get; set; } = new();
        public int? Quantity { get; set; }
    }

    public class AttributeOfItemDto
    {
        public long? Id { get; set; }
        public List<string>? ValueList { get; set; }
        public List<string>? UnitList { get; set; }
    }

    public enum EItemStatus // chỉ dùng cho HttpEcofarmItemService
    {
        PENDING = 1,
        ACTIVATED = 2, // đang hoạt động

        // VIOLATION = 3,   // vi phạm, bị tạm khóa 
        DEBOOSTED = 31,
        BANNED = 32,
        DELETED_BY_ADMIN = 33,

        DELETED = 4, // đã xóa (có thể khôi phục)
        HIDDEN = 5, //  trạng thái ẩn
        BLOCK = 6 // ngừng kinh doanh
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

    #region EcofarmPackageActivity

    public class EcofarmPackageActivityDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public long ProviderId { get; set; }
        public long EcofarmPackageId { get; set; }
        public string Name { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateExpect { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public List<string>? VideoUrlList { get; set; }
        public int Status { get; set; }
        public string? Description { get; set; }
        public int Type { get; set; }
        public string? Properties { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class GetAllEcofarmPackageActivitiesDto : CommonInputDto
    {
        public int? Type { get; set; }
        public long? ProviderId { get; set; }
        public long? EcofarmPackageId { get; set; }
        public DateTime? DateStartFrom { get; set; }
        public DateTime? DateStartTo { get; set; }
        public DateTime? DateExpectFrom { get; set; }
        public DateTime? DateExpectTo { get; set; }
    }

    public class CreateEcofarmPackageActivitiesDto
    {
        public int? TenantId { get; set; }
        public long ProviderId { get; set; }
        public long EcofarmPackageId { get; set; }
        public string Name { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateExpect { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public List<string>? VideoUrlList { get; set; }
        public string? Description { get; set; }
        public int Type { get; set; }
        public string? Properties { get; set; }
    }

    public class UpdateEcofarmPackageActivityDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public long? ProviderId { get; set; }
        public long? EcofarmPackageId { get; set; }
        public string? Name { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateExpect { get; set; }
        public List<string>? ImageUrlList { get; set; }
        public List<string>? VideoUrlList { get; set; }
        public string? Description { get; set; }
        public int? Type { get; set; }
        public string? Properties { get; set; }
    }
    public class UpdateStatusEcofarmPackageActivityDto: EntityDto<long>
    {
        public int Status { get; set; }
    }
    public class DeleteEcofarmPackageActivityDto : Entity<long>
    {
    }

    public class DeleteManyEcofarmPackageActivityDto
    {
        public List<long>? Ids { get; set; }
        public long? ProviderId { get; set; }
    }

    #endregion

    #region Cart

    public class CartItemModel
    {
        public ItemModelDto? ItemModel { get; set; }
        public int? Quantity { get; set; }
        public long? ProviderId { get; set; }
        public string? ItemName { get; set; }
        public string? ProviderName { get; set; }
    }

    public class ItemModel
    {
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

        public List<int> TierIndex { get; set; } = new();

        // 3 trường ko map sang PItemMOdel
        public bool IsDeleted { get; set; }
        public DateTime? DeletionTime { get; set; }
        public long? DeleterUserId { get; set; }
    }

    public class GetCartForEcoFarmDto : CommonInputDto
    {
    }

    public class Cart
    {
        public long UserId { get; set; }

        // List CartItem
        public string? Items { get; set; }
    }

    public class AddItemModelToCartForEcoFarmDto
    {
        public long ItemModelId { get; set; }
        public int Quantity { get; set; }
        public long ProviderId { get; set; }
    }

    public class UpdateCartEcoFarmDto
    {
        public List<CartItemDto> Items { get; set; }
    }

    public class CartItemDto
    {
        public long ItemModelId { get; set; }
        public int Quantity { get; set; }
        public long ProviderId { get; set; }
    }

    #endregion
}