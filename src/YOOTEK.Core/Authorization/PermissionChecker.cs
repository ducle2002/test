using Abp.Authorization;
using IMAX.Authorization.Roles;
using IMAX.Authorization.Users;

namespace IMAX.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
