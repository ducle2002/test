using Abp.Dependency;
using Abp.Quartz;
using Yootek.Yootek.Services.Yootek.DichVu.Payment;
using Yootek.Services;
using Quartz;
using System.Threading.Tasks;

namespace Yootek.Web.Host.Quartzs.Jobs
{
    public class BillReminderJob : JobBase, ITransientDependency
    {
        private readonly IPaymentAppService _service;
        public BillReminderJob(
            IPaymentAppService service
            )
        {
            _service = service;
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            await _service.RemindUserBill();
        }
    }

}
