using Abp.Modules;
using Abp.Quartz;
using Abp.Reflection.Extensions;
using Yootek.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using Abp.Timing;

namespace Yootek.Web.Host.Startup
{
    [DependsOn(
        typeof(YootekWebCoreModule),
        typeof(AbpQuartzModule)
        )
        ]
    public class YootekWebHostModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public YootekWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void PreInitialize()
        {
            // Configuration.BackgroundJobs.UseHangfire();
            Clock.Provider = ClockProviders.Utc;
        }


        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(YootekWebHostModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            // Quartz scheduler
            var _quartzScheduler = IocManager.Resolve<IQuartzScheduler>();
            _quartzScheduler.Init();
        }
    }
}