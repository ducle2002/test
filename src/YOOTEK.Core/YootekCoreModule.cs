using Abp.Localization;
using Abp.MailKit;
using Abp.Modules;
using Abp.Net.Mail.Smtp;
using Abp.Net.Mail;
using Abp.Reflection.Extensions;
using Abp.Timing;
using Abp.Zero;
using Abp.Zero.Configuration;
using Yootek.Authorization.Roles;
using Yootek.Authorization.Users;
using Yootek.Configuration;
using Yootek.Localization;
using Yootek.MultiTenancy;
using Yootek.Timing;
using Castle.MicroKernel.Registration;
using Yootek.Emailing;

namespace Yootek
{
    [DependsOn(
        typeof(AbpMailKitModule),
        typeof(AbpZeroCoreModule))]
    public class YootekCoreModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Auditing.IsEnabledForAnonymousUsers = true;
            // Declare entity types
            Configuration.Modules.Zero().EntityTypes.Tenant = typeof(Tenant);
            Configuration.Modules.Zero().EntityTypes.Role = typeof(Role);
            Configuration.Modules.Zero().EntityTypes.User = typeof(User);

            YootekLocalizationConfigurer.Configure(Configuration.Localization);

            // Enable this line to create a multi-tenant application.
            Configuration.MultiTenancy.IsEnabled = YootekConsts.MultiTenancyEnabled;

            // Configure roles
            AppRoleConfig.Configure(Configuration.Modules.Zero().RoleManagement);

            Configuration.ReplaceService(typeof(IEmailSenderConfiguration), () =>
            {
                Configuration.IocManager.IocContainer.Register(
                    Component.For<IEmailSenderConfiguration, ISmtpEmailSenderConfiguration>()
                             .ImplementedBy<YootekSmtpEmailSenderConfiguration>()
                             .LifestyleTransient()
                );
            });

            Configuration.Settings.Providers.Add<AppSettingProvider>();

            Configuration.Localization.Languages.Add(new LanguageInfo("en", "English", "famfamfam-flags us"));
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(YootekCoreModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            IocManager.Resolve<AppTimes>().StartupTime = Clock.Now;
        }
    }
}
