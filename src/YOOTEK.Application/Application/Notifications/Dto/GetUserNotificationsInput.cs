using Abp.Notifications;
using Yootek.Common;
using System;

namespace Yootek.Notifications.Dto
{
    public class GetUserNotificationsInput : CommonInputDto
    {
        public UserNotificationState? State { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}