using Abp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Notifications
{
    public class UserPostMessageNotificationData : NotificationData
    {
        public string Action { get; set; }
        public string Icon { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public long? PostId { get; set; }
        public long? CommentId { get; set; }
        public long? LikeId { get; set; }
        public string DetailUrlApp { get; set; }
        public string DetailUrlWA { get; set; }

        public UserPostMessageNotificationData(string action, string icon, long postId, long? commentId, long? likeId, string message, string detailUrlApp, string detailUrlWA, string imageUrl = "", string description = "")
        {
            Action = action;
            Icon = icon;
            ImageUrl = imageUrl;
            Description = description;
            Message = message;
            PostId = postId;
            LikeId = likeId;
            CommentId = commentId;
            DetailUrlApp = detailUrlApp;
            DetailUrlWA = detailUrlWA;
            this["Message"] = message;
            this["Icon"] = Icon;
            this["ImageUrl"] = ImageUrl;
            this["Description"] = Description;
            this["PostId"] = PostId;
            this["CommentId"] = CommentId;
            this["LikeId"] = LikeId;
            this["DetailUrlApp"] = DetailUrlApp;
            this["DetailUrlWA"] = DetailUrlWA;
        }
    }

    public class UserInfo
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
    }

}
