using Abp.Application.Services;
using Abp.Authorization;
using IMAX.Authorization.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Authorization
{
    public interface ICheckTokenAppService : IApplicationService
    {

    }

    [AbpAuthorize]
    public class CheckTokenAppService : IMAXAppServiceBase, ICheckTokenAppService
    {
        public CheckTokenAppService() { }

        public async Task Check()
        {
            return;
        }

    }
}
