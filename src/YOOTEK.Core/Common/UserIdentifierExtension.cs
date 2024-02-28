using Abp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YOOTEK.Common
{
    public static class UserIdentifierExtension
    {

        public static string ToUserIdentifierStringNoti(this UserIdentifier user)
        {
            if (!user.TenantId.HasValue)
            {
                return user.UserId.ToString();
            }

            return user.UserId + "/" + user.TenantId;
        }
    }
}
