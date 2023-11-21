using Abp.EntityFrameworkCore.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Zero.EntityFrameworkCore;
using IMAX.EntityFrameworkCore.Seed;
using System;

namespace IMAX.EntityFrameworkCore
{
    [DependsOn(
        typeof(IMAXCoreModule),
        typeof(AbpZeroCoreEntityFrameworkCoreModule))]
    public class IMAXEntityFrameworkModule : AbpModule
    {
        /* Used it tests to skip dbcontext registration, in order to use in-memory database of EF Core */
        public bool SkipDbContextRegistration { get; set; }

        public bool SkipDbSeed { get; set; }

        public override void PreInitialize()
        {
            Configuration.Auditing.IsEnabled = false;
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            if (!SkipDbContextRegistration)
            {
                 Configuration.Modules.AbpEfCore().AddDbContext<IMAXDbContext>(options =>
                 
                 
                 {
                     if (options.ExistingConnection != null)
                     {
                         IMAXDbContextConfigurer.Configure(options.DbContextOptions, options.ExistingConnection);
                     }
                     else
                     {
                         IMAXDbContextConfigurer.Configure(options.DbContextOptions, options.ConnectionString);
                     }
                 });
            }
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(IMAXEntityFrameworkModule).GetAssembly());
           
        }

        public override void PostInitialize()
        {
            if (!SkipDbSeed)
            {
                SeedHelper.SeedHostDb(IocManager);
            }
        }
    }
}
