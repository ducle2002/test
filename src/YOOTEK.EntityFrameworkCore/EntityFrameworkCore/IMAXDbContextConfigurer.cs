using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace IMAX.EntityFrameworkCore
{
    public static class IMAXDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<IMAXDbContext> builder, string connectionString)
        {
            //builder.UseSqlServer(connectionString);
            builder.UseNpgsql(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<IMAXDbContext> builder, DbConnection connection)
        {
            //builder.UseSqlServer(connection);
            builder.UseNpgsql(connection);
        }
    }
}
