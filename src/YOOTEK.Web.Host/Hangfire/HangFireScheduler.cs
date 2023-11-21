using Abp.Dependency;
using Castle.Core.Logging;
using IMAX.IMAX.Services.IMAX.DichVu.Payment;
using IMAX.Services;
using IMAX.Users;

namespace IMAX.Web.Host
{
    public class HangFireScheduler : ISingletonDependency
    {
        public ILogger Logger { get; set; }

        public HangFireScheduler()
        {
            Logger = NullLogger.Instance;
        }

        private static object _syncObj = new object();
        private static object _syncObjBillDebt = new object();
        private static object _syncObjNoti = new object();

        public void HangFireReminderNotify()
        {
            lock (_syncObj)
            {
                using (var scope = IocManager.Instance.CreateScope())
                {
                    var _userDefaultService = scope.Resolve<IUserDefaultAppService>();
                    _userDefaultService.NotifyReminder();
                }
            }
        }

        public void HangfireReminderBillDebt()
        {
            lock (_syncObjNoti)
            {
                using (var scope = IocManager.Instance.CreateScope())
                {
                    var _reminderBillDebtService = scope.Resolve<IReminderBillDebtAppService>();
                    _reminderBillDebtService.ReminderUserBillDebtAsync();
                }
            }
        }

        public void HangFireDeleteAccount()
        {
            lock (_syncObj)
            {
                using (var scope = IocManager.Instance.CreateScope())
                {
                    var _userDefaultService = scope.Resolve<IUserDefaultAppService>();
                    _userDefaultService.HandleDeleteAccountTasks();
                }
            }
        }

        public void HangFireBillPaymentReminder()
        {
            lock (_syncObjBillDebt)
            {
                using (var scope = IocManager.Instance.CreateScope())
                {
                    var paymentAppService = scope.Resolve<IPaymentAppService>();
                    paymentAppService.RemindUserBill();
                }
            }
        }
    }
}