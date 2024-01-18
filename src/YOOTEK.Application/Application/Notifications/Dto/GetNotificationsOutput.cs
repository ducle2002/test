using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.Notifications;

namespace Yootek.Notifications.Dto
{
    public class GetNotificationsOutput 
    {
        public int UnreadCount { get; set; }
        public List<UserNotification> Data {  get; set; }
        public int TotalRecords { get; set; }

        public GetNotificationsOutput(int totalCount, int unreadCount, List<UserNotification> notifications)
        {
            UnreadCount = unreadCount;
            Data = notifications;
            TotalRecords = totalCount;
        }
    }

    public class GetNotificationsOutputOld : PagedResultDto<UserNotification>
    {
        public int UnreadCount { get; set; }

        public GetNotificationsOutputOld(int totalCount, int unreadCount, List<UserNotification> notifications)
            : base(totalCount, notifications)
        {
            UnreadCount = unreadCount;
        }
    }
}