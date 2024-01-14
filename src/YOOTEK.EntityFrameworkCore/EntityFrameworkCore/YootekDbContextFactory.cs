using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Yootek.Configuration;
using Yootek.Web;

namespace Yootek.EntityFrameworkCore
{
    /* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
    public class YootekDbContextFactory : IDesignTimeDbContextFactory<YootekDbContext>
    {
        public YootekDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<YootekDbContext>();
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            YootekDbContextConfigurer.Configure(builder, configuration.GetConnectionString(YootekConsts.ConnectionStringName));

            return new YootekDbContext(builder.Options);
        }
    }
}
