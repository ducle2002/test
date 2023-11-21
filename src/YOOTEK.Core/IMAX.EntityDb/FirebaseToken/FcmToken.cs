using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;

namespace IMAX.EntityDb
{
    [Table("FcmTokens")]
    public class FcmTokens : FullAuditedEntity<long>, IMayHaveTenant
    {
        public string Token { get; set; }
        [CanBeNull] public string DeviceId { get; set; }
        public int? DeviceType { get; set; }
        public int? TenantId { get; set; }
        public int? AppType { get; set; }
    }
}