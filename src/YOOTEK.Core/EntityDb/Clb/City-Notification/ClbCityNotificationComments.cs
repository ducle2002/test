using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Yootek.Yootek.EntityDb.Clb.City_Notification
{
    [Table("ClbCityNotificationComments")]
    public class ClbCityNotificationComment : FullAuditedEntity<long>, IMayHaveTenant
    {
        public string Comment { get; set; }
        public bool? IsLike { get; set; }
        public int? TenantId { get; set; }
        public long CityNotificationId { get; set; }
        public int? Type { get; set; }
    }
}