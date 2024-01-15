using Abp.Dependency;
using Abp.Quartz;
using Yootek.Notifications;
using Yootek.Services;
using Quartz;
using System.Threading.Tasks;

namespace Yootek.Web.Host.Quartzs.Jobs
{
    public class WorkCreateNotificationJob : JobBase, ITransientDependency
    {
        private readonly IWorkNotifyAppService _service;
        public WorkCreateNotificationJob(
            IWorkNotifyAppService service
            )
        {
            _service = service;
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            await _service.SchedulerWorkCreateNotificationAsync();
        }
    }
}
