using Yootek.Common;
using System;

namespace Yootek.Services.SmartSocial.Advertisements.Dto
{
    public class ApprovalAdvertisementInputDto
    {
        public long Id { get; set; }
    }
    public enum FORM_ID_USER
    {
        ACTIVED = 30
    }
    public enum FORM_ID_PARTNER
    {
        ALL = 20,
        PENDING = 21,
        ACTIVED = 22,
    }
    public enum FORM_ID_ADMIN
    {
        ALL = 10,
        PENDING = 11,
        ACTIVED = 12,
    }
    public enum ADVERTISEMENT_STATUS
    {
        PENDING = 1,
        ACTIVE = 2,
        EXPIRED = 3,
    }
    public class GetAllAdvertisementsByPartnerInputDto : CommonInputDto
    {
        public long? ProviderId { get; set; }
        public long? ItemId { get; set; }
        public FORM_ID_PARTNER? FormId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public long? CategoryId { get; set; }
        public long? TypeBusiness { get; set; }
    }
    public class GetAllAdvertisementsInputDto : CommonInputDto
    {
        public long? PartnerId { get; set; }
        public long? ProviderId { get; set; }
        public long? ItemId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public long? CategoryId { get; set; }
        public long? TypeBusiness { get; set; }
    }
    public class CreateAdvertisementInputDto
    {
        public int? TenantId { get; set; }
        public string Link { get; set; }
        public string ImageUrl { get; set; }
        public string? Descriptions { get; set; }
        public long? ProviderId { get; set; }
        public long? ItemId { get; set; }
        public long? CategoryId { get; set; }
        public long? TypeBusiness { get; set; }
    }
    public class DeleteAdvertisementInputDto
    {
        public long Id { get; set; }
    }
}
