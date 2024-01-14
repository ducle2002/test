using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("WorkLogTimes")]
    public class WorkLogTime : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long WorkId { get; set; }
        public long WorkDetailId { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateFinish { get; set; }
        public long UserId { get; set; }
        public LogTimeStatus Status { get; set; }
        public string Note { get; set; }
        public List<string> ImageUrls { get; set; }
    }

    public enum LogTimeStatus
    {
        NOT_STARTED = 1,  // chưa bắt đầu
        IN_PROGRESS = 2,  // đang thực hiện
        PAUSED = 3,  // tạm dừng
        COMPLETED = 4,  // hoàn thành
        CANCELLED = 5,  // hủy bỏ
        PENDING_APPROVAL = 6,  // chờ duyệt
        EXPIRED = 7,  // hết hạn
    }
}
