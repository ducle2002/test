using Abp.Dependency;
using Abp.Quartz;
using Yootek.Notifications;
using Quartz;
using System.Threading.Tasks;

namespace Yootek.Web.Host.Quartzs.Jobs
{
    public class YearCreateNotificationJob : JobBase, ITransientDependency
    {
        private readonly IAdminNotificationAppService _service;
        public YearCreateNotificationJob(
            IAdminNotificationAppService service
            )
        {
            _service = service;
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            await _service.SchedulerYearCreateNotificationAsync();
        }
    }
}
