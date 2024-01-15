using Common.Logging;
using Yootek.App.ServiceHttpClient.Business;
using Yootek.App.ServiceHttpClient.Social;
using Yootek.App.ServiceHttpClient.Yootek.SmartCommunity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Yootek.App.ServiceHttpClient
{
    public static class ServiceHttpClientConfigure
    {
        public static void ConfigureServiceHttpClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<LoggingDelegatingHandler>();
            //dependency HttpClient
            services.AddHttpClient<IHttpPaymentService, HttpPaymentService>(c =>
                c.BaseAddress = new Uri(configuration["ApiSettings:Business.Payment"])
            );

            services.AddScoped<IHttpAdvertisementService, HttpAdvertisementService>();

            services.AddHttpClient<IHttpQRCodeService, HttpQRCodeService>(c =>
                c.BaseAddress = new Uri(configuration["ApiSettings:QRCode"])
            );
            services.AddHttpClient<IHttpWorkAssignmentService, HttpWorkAssignmentService>(c =>
                c.BaseAddress = new Uri(configuration["ApiSettings:WorkAssignment"])
            );
            services.AddHttpClient<IVietNamAdministrativeUnitService, VietNamAdministrativeUnitService>(c =>
                c.BaseAddress = new Uri(configuration["ApiSettings:VietNamAdministrativeUnit"])
            );
            services.AddHttpClient<IHttpNotificationService, HttpNotificationService>(c =>
                c.BaseAddress = new Uri(configuration["ApiSettings:Business.Notification"])
            );
            services.AddHttpClient<IHttpReportService, HttpReportService>(c =>
                c.BaseAddress = new Uri(configuration["ApiSettings:Business.Report"])
            );
            services.AddHttpClient<IHttpSocialMediaService, HttpSocialMediaService>(c =>
                c.BaseAddress = new Uri(configuration["ApiSettings:SocialMedia"])
            );
            services.AddHttpClient<IHttpCategoryService, HttpCategoryService>(c =>
                c.BaseAddress = new Uri(configuration["ApiSettings:Business.Item"])
            );
            services.AddHttpClient<IHttpItemAttributeService, HttpItemAttributeService>(c =>
                c.BaseAddress = new Uri(configuration["ApiSettings:Business.Item"])
            );
            
         
        }

    }
}