using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;


namespace Yootek.EntityDb
{
    public class BusinessNotify : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public string Message { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }
}
