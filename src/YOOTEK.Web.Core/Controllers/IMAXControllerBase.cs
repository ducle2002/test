using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace IMAX.Controllers
{
    public abstract class IMAXControllerBase : AbpController
    {
        public static Benchmark mb = new Benchmark();
        protected IMAXControllerBase()
        {
            LocalizationSourceName = IMAXConsts.LocalizationSourceName;
        }


    }
}
