using Abp.Application.Services.Dto;
using Yootek.Common;
using System;
using System.Collections.Generic;

namespace Yootek.App.ServiceHttpClient.Dto.Business
{
    public class ProviderDto : EntityDto<long>
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
        public bool? IsDataStatic { get; set; }
        public bool? IsAdminCreate { get; set; }
        public string? DistrictId { get; set; }
        public string? ProvinceId { get; set; }
        public string? WardId { get; set; }
        public string? Address { get; set; }
        public string? WorkTime { get; set; }
        public long? OwnerId { get; set; }
        public int? ServiceType { get; set; }
        public DateTime CreationTime { get;set; }
        public long CreatorUserId { get; set; }
        public double? Distance { get; set; }
    }

    public class GetListEcofarmProvidersByPartnerDto : CommonInputDto
    {
        public int? Type { get; set; }
        public int? GroupType { get; set; }
        public int? OrderBy { get; set; }
        public int? FormId { get; set; }
    }

    public class GetListEcofarmProvidersByUserDto : CommonInputDto
    {
        public int? Type { get; set; }
        public int? GroupType { get; set; }
        public int? OrderBy { get; set; }
        public int? FormId { get; set; }
    }

    public class GetProviderEcofarmByIdDto : EntityDto<long>
    {
    }

    public class CreateProviderEcofarmDto
    {
        public string Name { get; set; }
        public string? Email { get; set; }
        public string? Contact { get; set; }
        public string? Description { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string>? ImageUrls { get; set; }
        public string? OwnerInfo { get; set; }
        public int Type { get; set; }
        public int GroupType { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Properties { get; set; }
        public string? DistrictId { get; set; }
        public string? ProvinceId { get; set; }
        public string? WardId { get; set; }
        public string? Address { get; set; }
    }

    public class UpdateProviderEcofarmDto : EntityDto<long>
    {
        public string Name { get; set; }
        public string? Email { get; set; }
        public string? Contact { get; set; }
        public string? Description { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string>? ImageUrls { get; set; }
        public string? OwnerInfo { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Properties { get; set; }
        public string? DistrictId { get; set; }
        public string? ProvinceId { get; set; }
        public string? WardId { get; set; }
        public string? Address { get; set; }
    }

    public class UpdateStateProviderEcofarmDto : EntityDto<long>
    {
        public int State { get; set; }
    }

    public class DeleteProviderEcofarmDto : EntityDto<long>
    {
    }

    public class EcoFarmProviderGetListDto : EntityDto<long>
    {
        public string Name { get; set; }
        public string? Email { get; set; }
        public string? Description { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string>? ImageUrls { get; set; }
        public string? OwnerInfo { get; set; }
        public int Type { get; set; }
        public int GroupType { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Properties { get; set; }
        public int State { get; set; }
        public string? DistrictId { get; set; }
        public string? ProvinceId { get; set; }
        public string? WardId { get; set; }
        public string? Address { get; set; }
        public long OwnerId { get; set; }
        public DateTime CreationTime { get; set; }
        public double? Distance { get; set; }
        public long CountRate { get; set; }
        public double RatePoint { get; set; }
    }
    
    public class EcoFarmProviderDetailDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string? Email { get; set; }
        public string? Description { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string>? ImageUrls { get; set; }
        public string? OwnerInfo { get; set; }
        public int Type { get; set; }
        public int GroupType { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Properties { get; set; }
        public int State { get; set; }
        public string? DistrictId { get; set; }
        public string? ProvinceId { get; set; }
        public string? WardId { get; set; }
        public string? Address { get; set; }
        public long CountRate { get; set; }
        public double RatePoint { get; set; }
        public long OwnerId { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
