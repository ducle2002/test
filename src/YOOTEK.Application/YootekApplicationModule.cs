using Abp.AutoMapper;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.MailKit;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Yootek.Authorization;
using Yootek.MailBauilder;

namespace Yootek
{
    [DependsOn(
        typeof(AbpMailKitModule),
        typeof(YootekCoreModule),
        typeof(AbpAutoMapperModule))]
    public class YootekApplicationModule : AbpModule
    {
        public YootekApplicationModule(
            )
        {
        }

        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<YootekAuthorizationProvider>();
            Configuration.ReplaceService<IMailKitSmtpBuilder, CustomMailKitSmtpBuilder>();
            //Adding custom AutoMapper mappings
            Configuration.Modules.AbpAutoMapper().Configurators.Add(mapper =>
            {
                CustomDtoMapper.CreateMappings(mapper);
            });
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(YootekApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );


        }
    }
}
