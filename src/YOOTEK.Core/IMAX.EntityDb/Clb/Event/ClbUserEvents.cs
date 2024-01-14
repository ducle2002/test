using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Yootek.Yootek.EntityDb.Clb.Event
{
    [Table("ClbUserEvents")]
    public class ClbUserEvent : Entity<long>, IDeletionAudited
    {
        public long UserId { get; set; }
        public long EventId { get; set; }
        public long? DeleterUserId { get; set; }
        public DateTime? DeletionTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}