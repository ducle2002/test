using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.EntityDb;

namespace Yootek.Yootek.EntityDb.Clb.City_Notification
{
    [Table("ClbCityNotifications")]
    public class ClbCityNotification : CreationAuditedEntity<long>, IMayHaveTenant, IDeletionAudited
    {
        [StringLength(2000)]
        public string Name { get; set; }
        public string Data { get; set; }
        [StringLength(2000)]
        public string FileUrl { get; set; }
        public int? Type { get; set; }
        public int? TenantId { get; set; }
        public long? DeleterUserId { get; set; }
        public DateTime? DeletionTime { get; set; }
        public bool IsDeleted { get; set; }
        public int? Follow { get; set; }
        public string ReceiverGroupCode { get; set; }
        public string DepartmentCode { get; set; }
        public string Department { get; set; }
        public int? State { get; set; }
        public bool? IsAllowComment { get; set; }
        public RECEIVE_TYPE? ReceiveAll { get; set; }
        public bool? IsReceiveAll { get; set; }

    }
}