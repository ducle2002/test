using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using static Yootek.Notifications.AppNotifier;

namespace Yootek.EntityDb
{
    [Table("FcmTokens")]
    public class FcmTokens : FullAuditedEntity<long>
    {
        public string Token { get; set; }
        [CanBeNull] public string DeviceId { get; set; }
        public int? DeviceType { get; set; }
        public int? TenantId { get; set; }
        public AppType? AppType { get; set; }
    }

    public enum AppType
    {
        ALL = 0,
        USER = 1,
        SELLER = 2,
        IOC = 3,
        IOT = 4,
    }

    public enum DeviceType
    {
        IOS = 1,
        ANDROID = 2,
        WEB = 3
    }
}