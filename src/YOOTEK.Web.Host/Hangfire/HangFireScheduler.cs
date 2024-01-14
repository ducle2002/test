using Abp.Dependency;
using Castle.Core.Logging;
using Yootek.Yootek.Services.Yootek.DichVu.Payment;
using Yootek.Notifications;
using Yootek.Services;
using Yootek.Users;

namespace Yootek.Web.Host
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
        private static object _syncPaymentMonthly = new object();
        private static object _syncBillMonthly = new object();

        private static object _syncNotiDay = new object();
        private static object _syncNotiMonth = new object();
        private static object _syncNotiYear = new object();

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

        public void HangFireBillPaymentReminders()
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

        public void HangFireStatisticPaymentMonthly()
        {
            lock (_syncPaymentMonthly)
            {
                using (var scope = IocManager.Instance.CreateScope())
                {
                    var service = scope.Resolve<IStatisticBillAppService>();
                    service.ReportUserBillPaymentMonthlyScheduler();
                }
            }
        }

        public void SchedulerAutomaticCreateUserBillMonthly()
        {
            lock (_syncBillMonthly)
            {
                using (var scope = IocManager.Instance.CreateScope())
                {
                    var service = scope.Resolve<IBillUtilAppService>();
                    service.SchedulerCreateBillMonthly();
                }
            }
        }

        public void SchedulerDayCreateNotification()
        {
            lock (_syncNotiDay)
            {
                using (var scope = IocManager.Instance.CreateScope())
                {
                    var service = scope.Resolve<IAdminNotificationAppService>();
                    service.SchedulerDayCreateNotificationAsync();
                }
            }
        }

        public void SchedulerMonthCreateNotification()
        {
            lock (_syncNotiDay)
            {
                using (var scope = IocManager.Instance.CreateScope())
                {
                    var service = scope.Resolve<IAdminNotificationAppService>();
                    service.SchedulerMonthCreateNotificationAsync();
                }
            }
        }

        public void SchedulerYearCreateNotification()
        {
            lock (_syncNotiDay)
            {
                using (var scope = IocManager.Instance.CreateScope())
                {
                    var service = scope.Resolve<IAdminNotificationAppService>();
                    service.SchedulerYearCreateNotificationAsync();
                }
            }
        }
    }

}