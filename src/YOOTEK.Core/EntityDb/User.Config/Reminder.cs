using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace Yootek.EntityDb
{
    public class Reminder : Entity<long>, ICreationAudited, IMayHaveTenant
    {
        public DateTime TimeFinish { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public int? Level { get; set; }
        public string LoopDay { get; set; }
        public bool? IsNotify { get; set; }
        public bool? IsLoop { get; set; }
        public int State { get; set; }
        public int? TenantId { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
