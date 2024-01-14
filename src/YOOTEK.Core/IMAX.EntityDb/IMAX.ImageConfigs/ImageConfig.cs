using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;

namespace Yootek.Yootek.EntityDb
{
    public enum ImageConfigScope
    {
        Global = 0,
        Tenant = 1,
    }

    public enum ImageConfigType
    {
        Other = 0,

        YoolifeBanner = 1,
        YoolifeLogo = 2,
        YoolifeShoppingBanner = 3,
        YoolifeAvatar = 4,
        YoolifeLoginBackground = 20,

        YooSellerBanner = 5,
        YooSellerLogo = 6,
        YooSellerAvatar = 7,

        YooIotBanner = 8,
        YooIotLogo = 9,
        YooIotAvatar = 10,

        YooAdminBanner = 11,
        YooAdminLogo = 12,
        YooAdminAvatar = 13,

        YooSellerWebBanner = 14,
        YooSellerWebLogo = 15,
        YooSellerWebAvatar = 16,

        YooShoppingWebBanner = 17,
        YooShoppingWebLogo = 18,
        YooShoppingWebAvatar = 19,
    }

    [Table("ImageConfigs")]
    public class ImageConfig : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public string ImageUrl { get; set; }
        public ImageConfigType Type { get; set; }
        public ImageConfigScope Scope { get; set; }

        [Column(TypeName = "jsonb")]
        [CanBeNull]
        public string Properties { get; set; }
    }
}