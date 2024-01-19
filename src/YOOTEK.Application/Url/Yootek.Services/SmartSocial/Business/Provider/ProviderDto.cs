using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.Services.SmartSocial.Providers.Dto
{
    [Table("Providers")]
    public class Provider : FullAuditedEntity<long>, IMayHaveTenant
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Contact { get; set; }
        public string? Description { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string>? ImageUrls { get; set; }
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
        public int? CountRateDynamic { get; set; }
        public double? RatePointDynamic { get; set; }
        public bool? IsDataStatic { get; set; }
        public bool? IsAdminCreate { get; set; }
        public string? DistrictId { get; set; }
        public string? ProvinceId { get; set; }
        public string? WardId { get; set; }
        public string? Address { get; set; }
        public string? WorkTime { get; set; }
        public long? OwnerId { get; set; }
    }
    public class TypeOfProviderDto
    {
        public int Type { get; set; }
        public int Value { get; set; }
        public string Label { get; set; }
    }

    public class S3Settings
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string BucketName { get; set; }
        public string Region { get; set; }
        public string BaseUrl { get; set; }

    }
    public class GetListProvidersByAdminDto : CommonInputDto
    {
        public int? Type { get; set; }
        public int? GroupType { get; set; }
        public long? OwnerId { get; set; }
        public int? OrderBy { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? State { get; set; }
    }

    public class GetProvidersByPartnerInputDto : CommonInputDto
    {
        public int? TenantId { get; set; }
        public FORM_ID_PARTNER_GET_PROVIDER FormId { get; set; }
        public int? Type { get; set; }
        public int? GroupType { get; set; }
        public OrderByProvider? OrderBy { get; set; }
    }
    public enum OrderByProvider
    {
        DISTANCE_ASC = 1, // khoảng cách tăng dần
        DISTANCE_DESC = 2,  // khoảng cách giảm dần
        RATING_ASC = 3,  // số sao tăng dần (1 -> 5)
        RATING_DESC = 4,  // số sao giảm dần (5 -> 1)
        DATE_ASC = 5,    // ngày tạo từ cũ nhất đến mới nhất
        DATE_DESC = 6,   // ngày tạo từ mới nhất đến cũ nhất 
    }
    public class GetProvidersByUserInputDto : CommonInputDto
    {
        public int? TenantId { get; set; }
        public int? Type { get; set; }
        public int? GroupType { get; set; }
        public List<int>? OrderBy { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? MinRatePoint { get; set; }
        public List<int>? ListServiceType { get; set; } = new List<int>();
    }

    public class GetProvidersRandomInputDto : CommonInputDto
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
    public class GetProviderByIdInputDto
    {
        public long Id { get; set; }
        public bool? IsDataStatic { get; set; } = false;
    }
    public enum FORM_ID_ADMIN_GET_PROVIDER
    {
        FORM_ADMIN_GET_PROVIDER_GETALL = 10,
        FORM_ADMIN_GET_PROVIDER_PENDING = 11,
        FORM_ADMIN_GET_PROVIDER_ACTIVATED = 12,
        FORM_ADMIN_GET_PROVIDER_INACTIVATED = 13,
        FORM_ADMIN_GET_PROVIDER_HIDDEN = 14,
        FORM_ADMIN_GET_PROVIDER_BLOCKED = 15,
    }
    public enum FORM_ID_PARTNER_GET_PROVIDER
    {
        FORM_PARTNER_GET_PROVIDER_GETALL = 20,
        FORM_PARTNER_GET_PROVIDER_PENDING = 21,
        FORM_PARTNER_GET_PROVIDER_ACTIVATED = 22,
        FORM_PARTNER_GET_PROVIDER_INACTIVATED = 23,
        FORM_PARTNER_GET_PROVIDER_HIDDEN = 24,
        FORM_PARTNER_GET_PROVIDER_BLOCKED = 25,
    }

    public enum FORM_ID_USER_GET_PROVIDER
    {
        FORM_USER_GET_PROVIDER_GETALL = 30,
    }

    // 24 field 
    public class CreateProviderByAdminInputDto
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
        public string[] ImageUrls { get; set; }
        public string OwnerInfo { get; set; }
        public string BusinessInfo { get; set; }
        public int? SocialTenantId { get; set; }
        public int? Type { get; set; }
        public int? GroupType { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string PropertyHistories { get; set; }
        public string Properties { get; set; }
        public int? State { get; set; }
        public string StateProperties { get; set; }
        public string DistrictId { get; set; }
        public string ProvinceId { get; set; }
        public string WardId { get; set; }
        public string Address { get; set; }
        public string WorkTime { get; set; }
        public long? OwnerId { get; set; }
    }

    public class CreateListProvidersByAdminInputDto
    {
        public List<CreateProviderByAdminInputDto> Items { get; set; }
    }

    // 20 field 
    public class CreateProviderByPartnerInputDto
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
        public string[] ImageUrls { get; set; }
        public string OwnerInfo { get; set; }
        public string BusinessInfo { get; set; }
        public int? SocialTenantId { get; set; }
        public int? Type { get; set; }
        public int? GroupType { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Properties { get; set; }
        public string DistrictId { get; set; }
        public string ProvinceId { get; set; }
        public string WardId { get; set; }
        public string Address { get; set; }
        public string WorkTime { get; set; }
    }

    public enum PROVIDER_STATE
    {
        PENDING = 1,
        ACTIVATED = 2,
        INACTIVATED = 3,
        HIDDEN = 4,
        BLOCKED = 5,
    }

    // 25 field
    public class UpdateProviderByAdminInputDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
        public string[] ImageUrls { get; set; }
        public string OwnerInfo { get; set; }
        public string BusinessInfo { get; set; }
        public int? SocialTenantId { get; set; }
        public int? Type { get; set; }
        public int? GroupType { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string PropertyHistories { get; set; }
        public string Properties { get; set; }
        public int? State { get; set; }
        public string StateProperties { get; set; }
        public string DistrictId { get; set; }
        public string ProvinceId { get; set; }
        public string WardId { get; set; }
        public string Address { get; set; }
        public string WorkTime { get; set; }
        public long? OwnerId { get; set; }
    }
    public class UpdateStateProviderByAdminInputDto
    {
        public long Id { get; set; }
        public int FormId { get; set; }
    }

    public class UpdateStateOfProviderByPartnerInputDto
    {
        public long Id { get; set; }
        public int FormId { get; set; }
    }
    public enum FormIdUpdateState
    {
        SUSPENDED = 1,
        ACTIVED = 2,
        BLOCK = 3,
    }
    // 14 field
    public class UpdateProviderByPartnerInputDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
        public string[] ImageUrls { get; set; }
        public string BusinessInfo { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string DistrictId { get; set; }
        public string ProvinceId { get; set; }
        public string WardId { get; set; }
        public string Address { get; set; }
    }
    public enum TypeReport
    {
        DEFAULT = 0,
        FAKE_ACCOUNT = 1,
        IMPERSONATE_SOMEONE = 2,
        INCONSONANT_POST = 3,
        NEED_SOME_HELP = 4,
        HAUNT_OR_BULLY = 5,
        OTHERS = 6
    }
    public class Report : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long ProviderId { get; set; }
        public TypeReport TypeReport { get; set; }
        public string ReportMessage { get; set; }
        public List<string> ImageUrls { get; set; }
        public bool IsStatic { get; set; }
        public int State { get; set; }
    }
    public class CreateReportInputDto
    {
        public long ProviderId { get; set; }
        public TypeReport TypeReport { get; set; }
        public string ReportMessage { get; set; }
        public List<string>? ImageUrls { get; set; }
        public bool IsStatic { get; set; }
    }
    public class DeleteReportInputDto
    {
        public List<long> Ids { get; set; }
    }
    public class GetAllReportsByAdminInputDto : CommonInputDto
    {
        public long? ProviderId { get; set; }
        public bool? IsStatic { get; set; }
        public TypeReport? TypeReport { get; set; }
        public FORMID_ADMIN_REPORT FormId { get; set; }
    }
    public class ApprovalReportByAdminInputDto
    {
        public long Id { get; set; }
    }
    public class ApproveProviderInputDto
    {
        public long Id { get; set; }
    }
    public enum FORMID_ADMIN_REPORT
    {
        GETALL = 10,
        PENDING = 11,
        APPROVED = 12,
    }
    public enum FORMID_USER_REPORT
    {
        GETALL = 20,
        PENDING = 21,
        APPROVED = 22,
    }
}
