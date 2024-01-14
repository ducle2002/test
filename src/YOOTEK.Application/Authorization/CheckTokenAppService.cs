using Abp.Application.Services;
using Abp.Authorization;
using Yootek.Authorization.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Authorization
{
    public interface ICheckTokenAppService : IApplicationService
    {

    }

    [AbpAuthorize]
    public class CheckTokenAppService : YootekAppServiceBase, ICheckTokenAppService
    {
        public CheckTokenAppService() { }

        public async Task Check()
        {
            return;
        }

    }
}
