using Abp.Notifications;
using IMAX.Common;
using System;

namespace IMAX.Notifications.Dto
{
    public class GetUserNotificationsInput : CommonInputDto
    {
        public UserNotificationState? State { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}