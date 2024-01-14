using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace Yootek.Controllers
{
    public abstract class YootekControllerBase : AbpController
    {
        public static Benchmark mb = new Benchmark();
        protected YootekControllerBase()
        {
            LocalizationSourceName = YootekConsts.LocalizationSourceName;
        }


    }
}
