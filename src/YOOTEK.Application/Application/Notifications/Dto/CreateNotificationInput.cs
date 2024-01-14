using Yootek.Common;
using Yootek.Organizations;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Yootek.Application.Notifications.Dto
{
    public class CreateNotificationInput
    {
        public bool IsTenant { get; set; }
        public bool IsProvider { get; set; }
        public int? BusinessType { get; set; }
        public string Header { get; set; }
        public string Content { get; set; }
        public string Icon { get; set; }
        public List<long> UserIds { get; set; }
        public int? TenantId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public bool IsOnlyFirebase { get; set; }
        public NotificationType TypeNotification { get; set; }
        public TypeScheduler TypeScheduler { get; set; }
        public DateTime? Time { get; set; }
        public DateTime? DueDate { get; set; }
        public List<DateTime> ListTimes { get; set; }
        public bool IsScheduler { get; set; }   
        public bool? IsAllUser { get; set; }
        public bool? IsOnlyCitizen { get; set; }
    }

    public class GetAllSchedulerNotificationInput: CommonInputDto
    {

    }

 }
