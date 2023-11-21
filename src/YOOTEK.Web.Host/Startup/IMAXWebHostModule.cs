using Abp.Modules;
using Abp.Reflection.Extensions;
using Hangfire;
using Hangfire.Common;
using IMAX.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;

namespace IMAX.Web.Host.Startup
{
    [DependsOn(
        typeof(IMAXWebCoreModule))
        ]
    public class IMAXWebHostModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public IMAXWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void PreInitialize()
        {
            // Configuration.BackgroundJobs.UseHangfire();
            Configuration.Auditing.IsEnabled = false;
        }


        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(IMAXWebHostModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            var appFolders = IocManager.Resolve<AppFolders>();
            appFolders.TempFileDownloadFolder = "C://Download";

            try
            {
                #region Hangfire

                var manager = new RecurringJobManager();
                //manager.AddOrUpdate("HangFileReminderNotify"
                //    , Job.FromExpression(() => new HangFireScheduler().HangFireReminderNotify())
                //    , Cron.Minutely());

                manager.AddOrUpdate("HangFireDeleteAccount"
                    , Job.FromExpression(() => new HangFireScheduler().HangFireDeleteAccount())
                    , Cron.Hourly());

                manager.AddOrUpdate("ScheduleCheckAllUserBillDebt"
                    , Job.FromExpression(() => new HangFireScheduler().HangFireBillPaymentReminder())
                    , Cron.Daily(6));

                //manager.AddOrUpdate("HangFireReminderBillDebt"
                //    , Job.FromExpression(() => new HangFireScheduler().HangfireReminderBillDebt())
                //    , Cron.Daily(7));

                #endregion

                //DirectoryHelper.CreateIfNotExists(appFolders.TempFileDownloadFolder);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message + "hangfire exception");
            }
        }
    }
}