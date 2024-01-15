using Abp.Dependency;
using Abp.Quartz;
using Yootek.Web.Host.Quartzs.Jobs;
using Quartz;
using System.Threading.Tasks;

namespace Yootek.Web.Host
{

    public interface IQuartzScheduler : ISingletonDependency
    {
        Task Init();
    }

    public class QuartzScheduler : IQuartzScheduler
    {
        private readonly IQuartzScheduleJobManager _jobManager;

        public QuartzScheduler(
            IQuartzScheduleJobManager jobManager
            )
        {
            _jobManager = jobManager;
        }

        public Task Init()
        {
            BillDebtReminderJobScheduler();
            BillReminderJobScheduler();
            CreateUserBillMonthlyJobScheduler();
           // DayCreateNotificationJobScheduler();
            MonthCreateNotificationJobScheduler();
            YearCreateNotificationJobScheduler();
            WorkCreateNotificationJobScheduler();

            return Task.CompletedTask;
        }

        protected Task BillDebtReminderJobScheduler()
        {
            _jobManager.ScheduleAsync<BillDebtReminderJob>(
                job => { job.WithIdentity("BillDebtReminderJobIdentity", "UserBill"); },
                trigger =>
                {
                    trigger.StartNow()
                        .WithCronSchedule("0 0 7 1/1 * ? *") 
                        .Build();
                });
            return Task.CompletedTask;
        }

        protected Task BillReminderJobScheduler()
        {
            _jobManager.ScheduleAsync<BillReminderJob>(
                job => { job.WithIdentity("BillReminderJobIdentity", "UserBill"); },
                trigger =>
                {
                    trigger.StartNow()
                        .WithCronSchedule("0 0 7 1/1 * ? *")
                        .Build();
                });
            return Task.CompletedTask;
        }

        protected Task CreateUserBillMonthlyJobScheduler()
        {
            _jobManager.ScheduleAsync<CreateUserBillMonthlyJob>(
                job => { job.WithIdentity("CreateUserBillMonthlyJobIdentity", "UserBill"); },
                trigger =>
                {
                    trigger.StartNow()
                        .WithCronSchedule("0 0 0 1/1 * ? *")
                        .Build();
                });
            return Task.CompletedTask;
        }

        protected Task DayCreateNotificationJobScheduler()
        {
            _jobManager.ScheduleAsync<DayCreateNotificationJob>(
                job => { job.WithIdentity("DayCreateNotificationJobIdentity", "Notification"); },
                trigger =>
                {
                    trigger.StartNow()
                        .WithCronSchedule("0 0 0/1 1/1 * ? *")
                        //.WithCronSchedule("0 0/1 * 1/1 * ? *")
                        .Build();
                });
            return Task.CompletedTask;
        }

        protected Task MonthCreateNotificationJobScheduler()
        {
            _jobManager.ScheduleAsync<MonthCreateNotificationJob>(
                job => { job.WithIdentity("MonthCreateNotificationJobIdentity", "Notification"); },
                trigger =>
                {
                    trigger.StartNow()
                        .WithCronSchedule("0 0 12 1/1 * ? *")
                        .Build();
                });
            return Task.CompletedTask;
        }

        protected Task YearCreateNotificationJobScheduler()
        {
            _jobManager.ScheduleAsync<YearCreateNotificationJob>(
                job => { job.WithIdentity("YearCreateNotificationJobIdentity", "Notification"); },
                trigger =>
                {
                    trigger.StartNow()
                        .WithCronSchedule("0 0 12 1 1/1 ? *")
                        .Build();
                });
            return Task.CompletedTask;
        }
        protected Task WorkCreateNotificationJobScheduler()
        {
            _jobManager.ScheduleAsync<WorkCreateNotificationJob>(
                job => { job.WithIdentity("WorkCreateNotificationJobIdentity", "Notification"); },
                trigger =>
                {
                    trigger.StartNow()
                        .WithCronSchedule("0 0/10 * * * ?")
                        .Build();
                });
            return Task.CompletedTask;
        }


    }

}
