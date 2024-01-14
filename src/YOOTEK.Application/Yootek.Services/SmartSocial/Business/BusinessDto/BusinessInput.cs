#nullable enable
using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using System;

namespace Yootek.Yootek.Services.Yootek.SmartSocial.BusinessNEW.BusinessDto
{
    public class ItemViewSettingInputDto : FilteredInputDto
    {
        public long? Id { get; set; }
        public int? FormId { get; set; }
        public int? FormCase { get; set; } //Điều kiện lọc 1
        public int? FormCase2 { get; set; } //Điều kiện lọc 2
        public int? Type { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public int? AttributeType { get; set; }
    }

    public class ItemTypeInputDto : CommonInputDto
    {
        public long? Id { get; set; }
        public int? FormId { get; set; }
        public int? FormCase { get; set; } //Điều kiện lọc 1
        public int? FormCase2 { get; set; } //Điều kiện lọc 2
        public int? Type { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
    }


    public class ItemsInputDto : FilteredInputDto
    {
        public long? Id { get; set; }
        public int? FormId { get; set; }
        public int? FormCase { get; set; } //Điều kiện lọc 1
        public int? FormCase2 { get; set; } //Điều kiện lọc 2
        public int? Type { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
    }

    public class ObjectInputDto : FilteredInputDto
    {
        public long? Id { get; set; }
        public int? FormId { get; set; }
        public int? FormCase { get; set; } //Điều kiện lọc 1
        public int? FormCase2 { get; set; } //Điều kiện lọc 2
        public int? Type { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
    }


    public class GetListRateInput : FilteredInputDto
    {
        public long? Id { get; set; }
        public int? FormId { get; set; }
        public int? FormCase { get; set; }
        public int? FormCase2 { get; set; }
        public int? Type { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
    }

    public class GetListOrderInput
    {
        public long? ObjectId { get; set; }
        public int? Type { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
    }

    public class GetAllOrderByMonthInput
    {
        public int NumberMonth { get; set; }
    }

    public class GetAllOrderByInput
    {
        public int? FormId { get; set; }
        public int MaxResultCount { get; set; }
    }

    public class ReportInputDto
    {
        public int? ObjectPartnerId { get; set; }
    }

    public class ReportOutputDto
    {
        public int? ObjectPartnerId { get; set; }
        public string NameObject { get; set; }
        public int? TypeService { get; set; }
        public int? TypeReason { get; set; }
        public string Detail { get; set; }
        public string? UserCreator { get; set; }
        public int? TenantId { get; set; }
    }

    public class StatisticsObjectInput
    {
        public int? Type { get; set; }
        public int NumberMonth { get; set; }
    }

    [AutoMap(typeof(ObjectPartner))]
    public class CreateObjectInputDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public int? SocialTenantId { get; set; }
        public int? Type { get; set; }
        public string QueryKey { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string PropertyHistories { get; set; }
        public string Properties { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Operator { get; set; }
        public int? Like { get; set; }
        public int? State { get; set; }
        public string StateProperties { get; set; }
        public int? CountRate { get; set; }
        public double? RatePoint { get; set; }
        public bool? IsDataStatic { get; set; }
        public bool? IsAdminCreate { get; set; }
        public string DistrictId { get; set; }
        public string ProvinceId { get; set; }
        public string WardId { get; set; }

        public int? GroupType { get; set; }

        // custome
        public bool IsTenantPartner { get; set; }
    }

    public class GetCategories : CommonInputDto
    {
        public long? ParentId { get; set; }
        public long? BusinessType { get; set; }
        public string? Search { get; set; }
    }

    public enum GetProviderOrderBy
    {
        Distance = 1,
        Rating = 2,
        Date = 3,
    }

    public class GetProvidersDto : CommonInputDto
    {
        public int? TenantId { get; set; }
        public long? UserId { get; set; }
        public long? Id { get; set; }
        public int? Type { get; set; }
        public int? GroupType { get; set; }
        public bool? IsDataStatic { get; set; }
        public GetProviderOrderBy? OrderBy { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class CreateProviderDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Contact { get; set; }
        public string? Description { get; set; }
        public string? PhoneNumber { get; set; }
        public string[]? ImageUrls { get; set; }
        public string? OwnerInfo { get; set; }
        public string? BusinessInfo { get; set; }
        public int? TenantId { get; set; }
        public int? SocialTenantId { get; set; }
        public int? Type { get; set; }
        public int? GroupType { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? PropertyHistories { get; set; }
        public string? Properties { get; set; }
        public int? State { get; set; }
        public string? StateProperties { get; set; }
        public int? CountRate { get; set; }
        public double? RatePoint { get; set; }
        public bool? IsDataStatic { get; set; }
        public bool? IsAdminCreate { get; set; }
        public string? DistrictId { get; set; }
        public string? ProvinceId { get; set; }
        public string? WardId { get; set; }
        public string? Address { get; set; }
        public string? WorkTime { get; set; }
        public bool? IsTenantPartner { get; set; }
        public long? OwnerId { get; set; }
    }

    public class UpdateProviderDto
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Contact { get; set; }
        public string? Description { get; set; }
        public string? PhoneNumber { get; set; }
        public string[]? ImageUrls { get; set; }
        public string? OwnerInfo { get; set; }
        public string? BusinessInfo { get; set; }
        public int? TenantId { get; set; }
        public int? SocialTenantId { get; set; }
        public int? Type { get; set; }
        public int? GroupType { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? PropertyHistories { get; set; }
        public string? Properties { get; set; }
        public int? State { get; set; }
        public string? StateProperties { get; set; }
        public int? CountRate { get; set; }
        public double? RatePoint { get; set; }
        public bool? IsDataStatic { get; set; }
        public bool? IsAdminCreate { get; set; }
        public string? DistrictId { get; set; }
        public string? ProvinceId { get; set; }
        public string? WardId { get; set; }
        public string? Address { get; set; }
        public string? WorkTime { get; set; }
        public bool? IsTenantPartner { get; set; }
        public long? OwnerId { get; set; }
    }
}