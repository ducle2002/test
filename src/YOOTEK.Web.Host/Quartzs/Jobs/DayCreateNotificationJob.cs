using Abp.Dependency;
using Abp.Quartz;
using Yootek.Notifications;
using Quartz;
using System.Threading.Tasks;

namespace Yootek.Web.Host.Quartzs.Jobs
{
    public class DayCreateNotificationJob : JobBase, ITransientDependency
    {
        private readonly IAdminNotificationAppService _service;
        public DayCreateNotificationJob(
            IAdminNotificationAppService service
            )
        {
            _service = service;
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            await _service.SchedulerDayCreateNotificationAsync();
        }
    }
}
