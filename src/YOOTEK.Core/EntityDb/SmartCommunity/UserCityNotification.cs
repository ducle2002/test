using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{

    [Table("UserCityNotification")]
    public class UserCityNotification : Entity<long>, IDeletionAudited
    {
        public long UserId { get; set; }
        public long CityNotificationId { get; set; }
        public long? DeleterUserId { get; set; }
        public DateTime? DeletionTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
