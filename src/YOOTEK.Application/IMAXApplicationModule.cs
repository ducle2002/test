using Abp.AutoMapper;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.MailKit;
using Abp.Modules;
using Abp.Reflection.Extensions;
using IMAX.Authorization;
using IMAX.MailBauilder;

namespace IMAX
{
    [DependsOn(
        typeof(AbpMailKitModule),
        typeof(IMAXCoreModule),
        typeof(AbpAutoMapperModule))]
    public class IMAXApplicationModule : AbpModule
    {
        public IMAXApplicationModule(
            )
        {
        }

        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<IMAXAuthorizationProvider>();
            Configuration.Auditing.IsEnabled = false;
            Configuration.ReplaceService<IMailKitSmtpBuilder, CustomMailKitSmtpBuilder>();
            //Adding custom AutoMapper mappings
            Configuration.Modules.AbpAutoMapper().Configurators.Add(mapper =>
            {
                CustomDtoMapper.CreateMappings(mapper);
            });
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(IMAXApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );


        }
    }
}
