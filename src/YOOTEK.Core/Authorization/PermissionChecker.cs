using Abp.Authorization;
using Yootek.Authorization.Roles;
using Yootek.Authorization.Users;

namespace Yootek.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
