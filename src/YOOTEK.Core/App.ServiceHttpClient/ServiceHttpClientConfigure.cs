using Common.Logging;
using IMAX.App.ServiceHttpClient.Imax.Business;
using IMAX.App.ServiceHttpClient.Imax.Social;
using IMAX.App.ServiceHttpClient.IMAX.SmartCommunity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Http;


namespace IMAX.App.ServiceHttpClient
{
    public static class ServiceHttpClientConfigure
    {
        public static void ConfigureServiceHttpClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<LoggingDelegatingHandler>();
            //dependency HttpClient
            services.AddHttpClient<IHttpOrderService, HttpOrderService>(c =>
                    c.BaseAddress = new Uri(configuration["ApiSettings:Business.Order"])
                )
                .AddHttpMessageHandler<LoggingDelegatingHandler>();
            services.AddHttpClient<IHttpPaymentService, HttpPaymentService>(c =>
                    c.BaseAddress = new Uri(configuration["ApiSettings:Business.Payment"])
                );

            services.AddHttpClient<IHttpAdvertisementService, HttpAdvertisementService>(c =>
                    c.BaseAddress = new Uri(configuration["ApiSettings:Business.Advertisement"])
                );

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
            services.AddHttpClient<IHttpInvestmentService, HttpInvestmentService>(c =>
                c.BaseAddress = new Uri(configuration["ApiSettings:Business.Item"])
            );
            services.AddHttpClient<IHttpCategoryService, HttpCategoryService>(c =>
                c.BaseAddress = new Uri(configuration["ApiSettings:Business.Item"])
            );
            services.AddHttpClient<IHttpItemAttributeService, HttpItemAttributeService>(c =>
               c.BaseAddress = new Uri(configuration["ApiSettings:Business.Item"])
           );
            // Health check API 
            //services.AddHealthChecks()
            //    .AddUrlGroup(new Uri($"{configuration["ApiSettings:Business.Order"]}/swagger/index.html"),
            //        "Catalog.API", HealthStatus.Degraded);
        }

        //private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        //{
        //    // In this case will wait for
        //    //  2 ^ 1 = 2 seconds then
        //    //  2 ^ 2 = 4 seconds then
        //    //  2 ^ 3 = 8 seconds then
        //    //  2 ^ 4 = 16 seconds then
        //    //  2 ^ 5 = 32 seconds

        //    return HttpPolicyExtensions
        //        .HandleTransientHttpError()
        //        .WaitAndRetryAsync(
        //            retryCount: 5,
        //            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        //            onRetry: (exception, retryCount, context) =>
        //            {
        //                Log.Error(
        //                    $"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception}.");
        //            });
        //}

        //private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        //{
        //    return HttpPolicyExtensions
        //        .HandleTransientHttpError()
        //        .CircuitBreakerAsync(
        //            handledEventsAllowedBeforeBreaking: 5,
        //            durationOfBreak: TimeSpan.FromSeconds(30)
        //        );
        //}
    }
}