using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public class Noti
    {
        public NotificationModel notification { get; set; }
    }

    public class NotificationModel
    {
        public string title { get; set; }
        public string body { get; set; }
        public string icon { get; set; }
    }

    public class BroadcastInput
    {
        public string Message { get; set; }
        public string Icon { get; set; }
        public string Title { get; set; }
    }

    public class ResponseModel
    {
        [JsonProperty("isSuccess")] public bool IsSuccess { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
    }


    public class PushNotificationItem
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Target { get; set; }
    }
}