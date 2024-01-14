
namespace Yootek.Common.Enum
{
    public static partial class UserFeedbackEnum
    {

        public enum STATE_VERTIFY
        {
            ACCEPT =  1, //cu dan da xac minh
            UNMATCH = 2, //tai khoan chua xac minh

        }

        public enum STATE_FEEDBACK
        {
            DECLINED = -1, //người dùng từ chối xác nhận từ admin
            PENDING = 1, //chờ xử lý - new
            HANDLING = 2, // đang xử lý
            ADMIN_CONFIRMED = 3, // admin xác nhận đã xử lý xong
            USER_CONFIRMED = 4, //người dùng xác nhận đã xử lý
            USER_RATE_FEEDBACK = 5, //người dùng đánh giá về feedback

            ASSIGNED = 6, // đã phân công phòng ban
            ASSIGNEDHANDLER = 7, // đã phân công người xử lý
            UPLOADREPORT = 8 // đã phân công người xử lý
        }


        public enum STATE_POST
        {
            NEW = 1,
            ACTIVE = 2,
            REFUSE = 3,
            ADMIN_DELETE = 4,
        }
        public enum FORM_ID_FEEDBACK
        {
            //admin
            [EnumDisplayString("Form admin get all feedback")]
            FORM_ADMIN_GET_FEEDBACK_GETALL = 10
           ,
            [EnumDisplayString("Form admin get new feedbacks")]
            FORM_ADMIN_GET_FEEDBACK_PENDING = 11
           ,
            [EnumDisplayString("Form admin get handling feedbacks")]
            FORM_ADMIN_GET_FEEDBACK_HANDLING = 12
           ,
            [EnumDisplayString("Form admin get declined feedbacks")]
            FORM_ADMIN_GET_FEEDBACK_DECLINED = 13
           ,
            [EnumDisplayString("Form admin get admin confirmed/ conplete feedbacks")]
            FORM_ADMIN_GET_FEEDBACK_ADMIN_CONFIRMED = 14
           ,
            [EnumDisplayString("Form admin get user rating feedbacks")]
            FORM_ADMIN_GET_FEEDBACK_USER_RATING = 15
           ,

            //user
            [EnumDisplayString("Form user get all feedback")]
            FORM_USER_GET_FEEDBACK_GETALL = 20
           ,
            [EnumDisplayString("Form user get new feedbacks")]
            FORM_USER_GET_FEEDBACK_PENDING = 21
           ,
            [EnumDisplayString("Form user get pending/handlding feedbacks")]
            FORM_USER_GET_FEEDBACK_HANDLING = 22
           ,
            [EnumDisplayString("Form user get handled feedbacks")]
            FORM_USER_GET_FEEDBACK_HANDLED = 23
           ,
        }

    }

}