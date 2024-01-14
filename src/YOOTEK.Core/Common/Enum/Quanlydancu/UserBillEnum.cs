
namespace Yootek.Common.Enum
{
    public static partial class CommonENumBill
    {

        public enum TYPEBILL
        {
            ELECTRIC_BILL = 1,
            WATER_BILL = 2,
            PARKING_BILL = 3

        }

        public enum FORM_ID_BILL
        {
            FORM_ADMIN_GETALL = 1,
            FORM_ADMIN_GETALL_NEW_MONTH = 11,
            FORM_ADMIN_GETALL_BY_MONTH = 12,
            FORM_ADMIN_GETALL_BY_USER = 14,
            FORM_ADMIN_GETALL_OLD = 15,
            FORM_ADMIN_GET_BY_ID = 12,
            FORM_ADMIN_GET_BY_USER = 13,
            FORM_USER_GETALL = 2,
            FORM_USER_GET_BY_ID = 21,
        }
    }
}
