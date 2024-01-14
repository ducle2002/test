using Abp.Dependency;
using Abp.Quartz;
using Yootek.Services;
using Quartz;
using System;
using System.Threading.Tasks;

namespace Yootek.Web.Host.Quartzs.Jobs
{
    public class CreateUserBillMonthlyJob : JobBase, ITransientDependency
    {
        private readonly IBillUtilAppService _service;
        public CreateUserBillMonthlyJob(
            IBillUtilAppService service
            )
        {
            _service = service;
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            await _service.SchedulerCreateBillMonthly();
        }
    }
}
