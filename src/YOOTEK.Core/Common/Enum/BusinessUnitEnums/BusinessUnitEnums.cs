using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Common.Enum.RegBusinessEnums
{
    public static partial class BusinessUnitEnums
    {
        public enum FORM_ID_OBJECTS
        {
            [EnumDisplayString("Form admin getall object")]
            FORM_ADMIN_GET_OBJECT_GETALL = 1,
            [EnumDisplayString("Form partner getall object ")]
            FORM_PARTNER_OBJECT_GETALL = 2,
            [EnumDisplayString("Form user getall object active")]
            FORM_USER_OBJECT_GETALL = 3,
            [EnumDisplayString("Form searching object")]
            FORM_SEARCHING_OBJECT = 4,
            [EnumDisplayString("Form get all object by ouID")]
            FORM_GET_ALL_UNIT_ID_OBJECT = 5,
            [EnumDisplayString("Form searching object in an ouID")]
            FORM_SEARCHING_UNIT_ID_OBJECT = 6,
        }
    }
}
