namespace Yootek.Common.Enum
{
    public static partial class CommonENum
    {

        public enum ROLE_LEVEL
        {
            [EnumDisplayString("ADMIN")]
            SA = 0,
            [EnumDisplayString("Quản lý cư dân")]
            CITIZEN_MANAGER = 1,

        }

        public enum STATE_REMINDER
        {
            ON = 1,
            OFF = 1
        }

        public enum ATTRIBUTE_TYPE
        {
            CATEGORY = 1,
            ATTRIBUTE = 2
        }

        public enum STATE_NEWS
        {
            SHOW = 1,
            HIDE = 2,
            HOTNEWS = 3
        }
        public enum SortBy
        {
            ASC = 1,
            DESC = 2,
        }

        public enum AssetStatus
        {
            ChoDuyet = 1,
            DaDuyet = 2,
        }
    }

    public static partial class CommonENumForum
    {
        public enum FORM_ID_FORUM
        {
            ADMIN_GETALL = 1,
            ADMIN_GETALL_NEW = 11,
            ADMIN_GETALL_ACCEPT = 12,
            ADMIN_GETALL_DISABLE = 13,
            USER_GETALL = 2,
            CREATOR_GETALL = 3
        }
        public enum FORUM_STATE
        {
            NEW = 1,
            ACCEPT = 2,
            DISABLE = 3,
            CLOSE = 4
        }
    }
}
