
namespace Yootek.Common.Enum
{
    public static partial class UserFeedbackCommentEnum
    {

        //public enum FORM_ID_FEEDBACK
        //{
        //    DECLINED = 0, //người dùng từ chối xác nhận từ admin
        //    PENDING = 1, //chờ xử lý
        //    HANDLING = 2, // đang xử lý
        //    ADMIN_CONFIRMED = 3, // admin xác nhận đã xử lý xong
        //    USER_CONFIRMED = 4, //người dùng xác nhận đã xử lý
        //    USER_RATE_FEEDBACK = 5, //người dùng đánh giá về feedback
        //}
        public enum STATE_READ_COMMENT_FEEDBACK
        {
            READ = 0,
            UN_READ = 1,
        }
        public enum TYPE_COMMENT_FEEDBACK
        {
            TEXT = 1, // tin nhắn text
            IMAGE = 2, // 1 hình ảnh
            VIDEO = 3,// 1 video
            FILE = 4, // 1 file
            IMAGES = 5, // nhiều hình ảnh
            VIDEOS = 6, // nhiều video
            FILES = 7, // nhiều file
            STATE_PENDING = 8, // phản ánh mới
            STATE_DECLINED = 9, //người dùng từ chối xác nhận từ admin 
            STATE_HANDLING = 10, // đang xử lý
            STATE_SETTIME = 14, // hẹn lịch xử lý
            STATE_ADMIN_CONFIRMED = 11, // admin xác nhận đã xử lý xong
            STATE_USER_CONFIRMED = 12, //người dùng xác nhận đã xử lý
            STATE_USER_RATE_FEEDBACK = 13, //người dùng đánh giá về feedback
        }
    }
}
