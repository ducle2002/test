using System;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Common;

namespace Yootek.App.ServiceHttpClient.Dto.Business
{
    public class Advertisement : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int Status { get; set; }
        public string Link { get; set; }
        public string ImageUrl { get; set; }
        public string? Descriptions { get; set; }
        public long PartnerId { get; set; }
        public long? ProviderId { get; set; }
        public long? ItemId { get; set; }
        public long? CategoryId { get; set; }
        public long? TypeBusiness { get; set; }
        public int? TenantId { get; set; }
    }

    public class AdvertisementDto : EntityDto<long>
    {
        public string Link { get; set; }
        public string ImageUrl { get; set; }
        public string? Descriptions { get; set; }
        public long PartnerId { get; set; }
        public long? ProviderId { get; set; }
        public long? ItemId { get; set; }
        public long? CategoryId { get; set; }
        public long? TypeBusiness { get; set; }
        public int Status { get; set; }
        public int? TenantId { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class GetAllAdvertisementsDto : CommonInputDto
    {
        public long? PartnerId { get; set; }
        public long? ProviderId { get; set; }
        public long? ItemId { get; set; }
        public int FormId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public long? CategoryId { get; set; }
        public long? TypeBusiness { get; set; }
    }

    public class GetAdvertisementByIdDto : EntityDto<long>
    {
    }
    
    public class CreateAdvertisementDto
    {
        public int? TenantId { get; set; }
        public string Link { get; set; }
        public string ImageUrl { get; set; }
        public string? Descriptions { get; set; }
        public long PartnerId { get; set; }
        public long? ProviderId { get; set; }
        public long? ItemId { get; set; }
        public long? CategoryId { get; set; }
        public long? TypeBusiness { get; set; }
    }

    public class UpdateAdvertisementDto : EntityDto<long>
    {
        public string? Link { get; set; }
        public string? ImageUrl { get; set; }
        public string? Descriptions { get; set; }
        public long? PartnerId { get; set; }
        public long? ProviderId { get; set; }
        public long? ItemId { get; set; }
        public long? CategoryId { get; set; }
        public long? TypeBusiness { get; set; }
    }

    public class UpdateStatusAdvertisementDto : EntityDto<long>
    {
        public int Status { get; set; }
    }

    public class DeleteAdvertisementDto : EntityDto<long>
    {
    }

    public class ApprovalAdvertisementDto : EntityDto<long>
    {
        public int Status { get; set; }
    }
}