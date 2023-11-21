using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using IMAX.Configuration;
using IMAX.Web;

namespace IMAX.EntityFrameworkCore
{
    /* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
    public class IMAXDbContextFactory : IDesignTimeDbContextFactory<IMAXDbContext>
    {
        public IMAXDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<IMAXDbContext>();
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            IMAXDbContextConfigurer.Configure(builder, configuration.GetConnectionString(IMAXConsts.ConnectionStringName));

            return new IMAXDbContext(builder.Options);
        }
    }
}
