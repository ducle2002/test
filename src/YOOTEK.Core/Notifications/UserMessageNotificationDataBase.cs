using Abp.Notifications;

namespace Yootek.Notifications
{
    public class UserMessageNotificationDataBase : NotificationData
    {
        static string DefaultImageUrl = "";
        public string Action { get; set; }
        public string Icon { get; set; }
        public TypeAction TypeAction { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public long DetailId { get; set; }
        public string DetailUrlApp { get; set; }
        public string DetailUrlWA { get; set; }
        public UserMessageNotificationDataBase(string action, string icon, TypeAction typeAction, string message, string detailUrlApp, string detailUrlWA, string imageUrl = "", string description = "", long detailId = 0)
        {
            Action = action;
            Icon = icon;
            TypeAction = typeAction;
            ImageUrl = imageUrl;
            Description = description;
            Message = message;
            DetailId = detailId;
            DetailUrlApp = detailUrlApp;
            DetailUrlWA = detailUrlWA;
            this["Message"] = message;
            this["Icon"] = Icon;
            this["TypeAction"] = TypeAction;
            this["ImageUrl"] = ImageUrl;
            this["Description"] = Description;
            this["DetailId"] = DetailId;
            this["DetailUrlApp"] = DetailUrlApp;
            this["DetailUrlWA"] = DetailUrlWA;
        }
    }

    public class UserMessageNotificationSeller : NotificationData
    {
        public string Action { get; set; }
        public string Icon { get; set; }
        public TypeAction TypeAction { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public long ProviderId { get; set; }
        public string DetailUrlApp { get; set; }
        public string DetailUrlWA { get; set; }
        public UserMessageNotificationSeller(string action, string icon, TypeAction typeAction, string message, string detailUrlApp, string detailUrlWA, string imageUrl = "", string description = "", long providerId = 0)
        {
            Action = action;
            Icon = icon;
            TypeAction = typeAction;
            ImageUrl = imageUrl;
            Description = description;
            Message = message;
            ProviderId = providerId;
            DetailUrlApp = detailUrlApp;
            DetailUrlWA = detailUrlWA;
            this["Message"] = message;
            this["Icon"] = Icon;
            this["TypeAction"] = TypeAction;
            this["ImageUrl"] = ImageUrl;
            this["Description"] = Description;
            this["ProviderId"] = ProviderId;
            this["DetailUrlApp"] = DetailUrlApp;
            this["DetailUrlWA"] = DetailUrlWA;
        }
    }


    public enum TypeAction
    {
        Detail = 1,
        ListView = 2
    }
}
