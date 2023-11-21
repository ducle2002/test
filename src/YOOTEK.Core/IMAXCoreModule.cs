using Abp.Localization;
using Abp.MailKit;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Timing;
using Abp.Zero;
using Abp.Zero.Configuration;
using IMAX.Authorization.Roles;
using IMAX.Authorization.Users;
using IMAX.Configuration;
using IMAX.Localization;
using IMAX.MultiTenancy;
using IMAX.Timing;

namespace IMAX
{
    [DependsOn(
        typeof(AbpMailKitModule),
        typeof(AbpZeroCoreModule))]
    public class IMAXCoreModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Auditing.IsEnabledForAnonymousUsers = true;
            Configuration.Auditing.IsEnabled = false;
            // Declare entity types
            Configuration.Modules.Zero().EntityTypes.Tenant = typeof(Tenant);
            Configuration.Modules.Zero().EntityTypes.Role = typeof(Role);
            Configuration.Modules.Zero().EntityTypes.User = typeof(User);

            IMAXLocalizationConfigurer.Configure(Configuration.Localization);

            // Enable this line to create a multi-tenant application.
            Configuration.MultiTenancy.IsEnabled = IMAXConsts.MultiTenancyEnabled;

            // Configure roles
            AppRoleConfig.Configure(Configuration.Modules.Zero().RoleManagement);

            Configuration.Settings.Providers.Add<AppSettingProvider>();

            Configuration.Localization.Languages.Add(new LanguageInfo("en", "English", "famfamfam-flags us"));
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(IMAXCoreModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            IocManager.Resolve<AppTimes>().StartupTime = Clock.Now;
        }
    }
}
