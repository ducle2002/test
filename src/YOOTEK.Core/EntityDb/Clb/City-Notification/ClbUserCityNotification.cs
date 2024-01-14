using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Yootek.Yootek.EntityDb.Clb.City_Notification
{
    [Table("ClbUserCityNotifications")]
    public class ClbUserCityNotification : Entity<long>, IDeletionAudited
    {
        public long UserId { get; set; }
        public long CityNotificationId { get; set; }
        public long? DeleterUserId { get; set; }
        public DateTime? DeletionTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}