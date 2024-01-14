using Yootek.BackgroundJob;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace Yootek.Web.Host.Startup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    //services.AddHostedService<TimedHostedService>();
                    services.AddHostedService<QueuedHostedService>();
                })
                .UseStartup<Startup>()
                .Build();
        }

    }
}
