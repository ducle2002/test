namespace Yootek.Notifications
{
    public class NotificationWithContentIdDatabase: UserMessageNotificationDataBase
    {
        static string DefaultImageUrl = "";

        public long ContentId { get; set; }
      // public string DetailUrlApp { get; set; }

        public NotificationWithContentIdDatabase(long contentId, string action,  string icon, TypeAction typeAction, string message, string detailUrlApp, string detailUrlWA, string imageUrl = "", string description = ""): base(action, icon, typeAction, message, detailUrlApp, detailUrlWA, imageUrl, description)
        {
            ContentId = contentId;
         //   DetailUrlApp = detailUrlApp;
            this["ContentId"] = ContentId;
           // this["DetailUrlApp"] = DetailUrlApp;

        }
    }
}
