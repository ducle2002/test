using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Yootek.EntityFrameworkCore
{
    public static class YootekDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<YootekDbContext> builder, string connectionString)
        {
            //builder.UseSqlServer(connectionString);
            builder.UseNpgsql(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<YootekDbContext> builder, DbConnection connection)
        {
            //builder.UseSqlServer(connection);
            builder.UseNpgsql(connection);
        }
    }
}
