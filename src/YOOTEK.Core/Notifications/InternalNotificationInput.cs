

using Abp;
using Abp.Notifications;
using Yootek.EntityDb;

namespace Yootek.Notifications
{
    public class InternalNotificationInput
    {
        public NotificationData NotificationMessageData {  get; set; }
        public UserIdentifier[] Users { get; set; }
        public string NotificationName { get; set; }
        public string FirebaseMessage {  get; set; }
        public string DetailUrlApp {  get; set; }
        public string DetailUrlWA { get; set; }
        public bool IsOnlyFirebase { get; set; }
        public AppType AppType { get; set; } = AppType.ALL;
        public NotificationSeverity? Severity { get; set; }
        public bool IsSendGroup {  get; set; }
        public string GroupName { get; set; }
        public long? ProviderId { get; set; }
    }
}
