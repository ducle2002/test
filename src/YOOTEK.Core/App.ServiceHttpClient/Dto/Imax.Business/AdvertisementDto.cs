using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using IMAX.Common;
using System;

namespace IMAX.App.ServiceHttpClient.Dto.Imax.Business
{
    public class Advertisement : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public int Status { get; set; }
        public string Link { get; set; }
        public string ImageUrl { get; set; }
        public string? Descriptions { get; set; }
        public long PartnerId { get; set; }
        public long? ProviderId { get; set; }
        public long? ItemId { get; set; }
        public long? CategoryId { get; set; }
        public long? TypeBusiness { get; set; }
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
    public class DeleteAdvertisementDto
    {
        public long Id { get; set; }
    }
    public class ApprovalAdvertisementDto
    {
        public long Id { get; set; }
        public int UpdateStatus { get; set; }
    }
}
