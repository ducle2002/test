using Abp.Dependency;
using Abp.Quartz;
using Yootek.Notifications;
using Quartz;
using System.Threading.Tasks;

namespace Yootek.Web.Host.Quartzs.Jobs
{
    public class MonthCreateNotificationJob : JobBase, ITransientDependency
    {
        private readonly IAdminNotificationAppService _service;
        public MonthCreateNotificationJob(
            IAdminNotificationAppService service
            )
        {
            _service = service;
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            await _service.SchedulerMonthCreateNotificationAsync();
        }
    }
}
