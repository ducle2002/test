
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;

namespace Yootek.EntityDb
{
    public class Rate : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long? ItemId { get; set; }
        public long? ObjectId { get; set; }
        public double? RatePoint { get; set; }
        public string FileUrl { get; set; }
        public string Comment { get; set; }
        [StringLength(256)]
        public string UserName { get; set; }
        [StringLength(100)]
        public string Email { get; set; }
        public long? AnswerRateId { get; set; }
    }
}
