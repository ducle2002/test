//using Abp.Dependency;
//using Abp.Quartz;
//using Yootek.Services;
//using Quartz;
//using System.Threading.Tasks;

//namespace Yootek.Web.Host.Quartzs.Jobs
//{
//    public class BillDebtReminderJob : JobBase, ITransientDependency
//    {
//        private readonly IReminderBillDebtAppService _service;
//        public BillDebtReminderJob(
//            IReminderBillDebtAppService service
//            )
//        {
//            _service = service;
//        }

//        public override async Task Execute(IJobExecutionContext context)
//        {
//            await _service.ReminderUserBillDebtAsync();
//        }
//    }
//}
