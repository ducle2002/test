using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;


namespace Yootek.EntityDb
{
    public class Voucher : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string Depscrtiption { get; set; }
        public long? Code { get; set; }
        public long? ObjectPartnerId { get; set; }
    }
}
