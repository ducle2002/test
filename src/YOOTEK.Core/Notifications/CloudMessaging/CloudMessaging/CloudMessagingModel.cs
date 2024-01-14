using Newtonsoft.Json;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Yootek.Notifications
{
    public class CloudMessagingModel
    {
        [JsonProperty("token")] public string Token { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("body")] public string Body { get; set; }
        [JsonProperty("icon")] public string Icon { get; set; }
    }

    public class FcmMultiSendToDeviceInput
    {
        [CanBeNull] public List<string> Tokens { get; set; }
        public int? TenantId { get; set; }
        [CanBeNull] public string GroupName { get; set; }
        public string Title { get; set; }
        [CanBeNull] public string SubTitle { get; set; }
        public string Body { get; set; }
        [CanBeNull] public string ClickAction { get; set; }
        [CanBeNull] public string Icon { get; set; }
        [CanBeNull] public string Data { get; set; }
    }


    public class FcmNotificationPayload
    {
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("subtitle")][CanBeNull] public string SubTitle { get; set; }
        [JsonProperty("body")] public string Body { get; set; }
        [JsonProperty("icon")][CanBeNull] public string Icon { get; set; }

        [JsonProperty("click_action")]
        [CanBeNull]
        public string ClickAction { get; set; }
    }

    public class FcmNotification
    {
        [JsonProperty("priority")][CanBeNull] public string Priority { get; set; } = "high";
        [JsonProperty("data")][CanBeNull] public object Data { get; set; }
        [JsonProperty("notification")] public FcmNotificationPayload Notification { get; set; }
        [JsonProperty("topic")][CanBeNull] public string Topic { get; set; }
        [JsonProperty("to")][CanBeNull] public string To { get; set; }

        [JsonProperty("registration_ids")]
        [CanBeNull]
        public List<string> RegistrationIds { get; set; }
    }

    public class FcmDeviceGroupDto
    {
        [JsonProperty(PropertyName = "operation", NullValueHandling = NullValueHandling.Ignore)]
        public string Operation { get; set; }

        [JsonProperty(PropertyName = "notification_key", NullValueHandling = NullValueHandling.Ignore)]
        public string NotificationKey { get; set; }

        [JsonProperty(PropertyName = "notification_key_name", NullValueHandling = NullValueHandling.Ignore)]
        public string NotificationKeyName { get; set; }

        [JsonProperty(PropertyName = "registration_ids", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> RegistrationIds { get; set; }

        public FcmDeviceGroupDto()
        {
            RegistrationIds = new List<string>();
        }
    }

    public class FcmNotificationSetting
    {
        public string SenderId { get; set; }
        public string ServerKey { get; set; }
    }

    public class FcmNotificationDojilandSetting
    {
        public string SenderId { get; set; }
        public string ServerKey { get; set; }
    }

    public class FcmAddDevicesToGroupInput
    {
        public List<string> Tokens { get; set; }
        public string NotificationKey { get; set; }
        public string Name { get; set; }
    }

    public class FcmRemoveDevicesFromGroupInput
    {
        public List<string> Tokens { get; set; }
        public string NotificationKey { get; set; }
        public string Name { get; set; }
    }
}