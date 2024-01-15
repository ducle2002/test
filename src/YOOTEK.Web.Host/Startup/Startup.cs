using Abp.AspNetCore;
using Abp.AspNetCore.Mvc.Antiforgery;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.Castle.Logging.Log4Net;
using Abp.Dependency;
using Abp.Extensions;
using Abp.Json;
using Castle.Facilities.Logging;
using CorePush.Apple;
using CorePush.Google;
using Yootek.App.ServiceHttpClient;
using Yootek.Application.Protos.Business.Bookings;
using Yootek.Application.Protos.Business.Items;
using Yootek.Application.Protos.Business.Orders;
using Yootek.Application.Protos.Business.Providers;
using Yootek.Application.Protos.Business.Rates;
using Yootek.Application.Protos.Business.Vouchers;
using Yootek.Authorization;
using Yootek.Configuration;
using Yootek.Identity;
using Yootek.Notifications;
using Yootek.Services.Notifications;
using Yootek.Web.Host.Chat;
using ImaxFileUploaderServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Reflection;
using System.IO;

namespace Yootek.Web.Host.Startup
{
    public class Startup
    {
        private const string _defaultCorsPolicyName = "localhost";

        private const string _apiVersion = "v1";

        private readonly IConfigurationRoot _appConfiguration;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public Startup(IWebHostEnvironment env)
        {
            _hostingEnvironment = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //MVC
            services.AddControllersWithViews(
                options => { options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute()); }
            ).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new AbpMvcContractResolver(IocManager.Instance);
                options.SerializerSettings.ContractResolver = new AbpMvcContractResolver(IocManager.Instance)
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };
            });
            services.AddTransient<IIpBlockingService, IpBlockingService>();
            services.AddScoped<IpBlockActionFilter>();

            //services.Configure<IdentityServerSettings>(Configuration.GetSection("IdentityServerSettings"));

            IdentityRegistrar.Register(services);
            AuthConfigurer.Configure(services, _appConfiguration);

            services.AddSignalR(e =>
            {
                e.MaximumReceiveMessageSize = 204800000;
                e.EnableDetailedErrors = true;
            });
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            // Configure CORS for angular2 UI
            services.AddCors(
                options => options.AddPolicy(
                    _defaultCorsPolicyName,
                    builder => builder
                        .WithOrigins(
                            // App:CorsOrigins in appsettings.json can contain more than one address separated by comma.
                            _appConfiguration["App:CorsOrigins"]
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.RemovePostFix("/"))
                                .ToArray()
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                )
            );

            services.AddSingleton<IAppNotifyBusiness, AppNotifyBusiness>();
            services.AddHttpClient();
            services.AddHttpClient<FcmSender>();
            services.AddHttpClient<ApnSender>();
            services.AddSingleton<IS3Service, S3Service>();
            // Configure strongly typed settings objects
            var appSettingsSection = _appConfiguration.GetSection("FcmNotification");
            services.Configure<FcmNotificationSetting>(appSettingsSection);

            services.Configure<FcmNotificationDojilandSetting>(_appConfiguration.GetSection("FcmDojiNotification"));
            //services.Configure<AbpMailKitOptions>(options =>
            //{
            //    options.SecureSocketOption = SecureSocketOptions.SslOnConnect;
            //});

            services.ConfigureServiceHttpClient(_appConfiguration);

            //service Session
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 2;
                options.SignIn.RequireConfirmedEmail = false;
                options.User.RequireUniqueEmail = false;
            });

            // Business 
            services.AddGrpcClient<ItemProtoGrpc.ItemProtoGrpcClient>(o =>
            {
                o.Address = new Uri(_appConfiguration["ServiceAddress:ItemProtoGrpc"]);
            });
            services.AddGrpcClient<ProviderProtoGrpc.ProviderProtoGrpcClient>(o =>
            {
                o.Address = new Uri(_appConfiguration["ServiceAddress:ProviderProtoGrpc"]);
            });
            services.AddGrpcClient<OrderProtoGrpc.OrderProtoGrpcClient>(o =>
            {
                o.Address = new Uri(_appConfiguration["ServiceAddress:OrderProtoGrpc"]);
            });
            services.AddGrpcClient<RateProtoGrpc.RateProtoGrpcClient>(o =>
            {
                o.Address = new Uri(_appConfiguration["ServiceAddress:RateProtoGrpc"]);
            });
            services.AddGrpcClient<VoucherProtoGrpc.VoucherProtoGrpcClient>(o =>
            {
                o.Address = new Uri(_appConfiguration["ServiceAddress:VoucherProtoGrpc"]);
            });
            services.AddGrpcClient<BookingProtoGrpc.BookingProtoGrpcClient>(o =>
            {
                o.Address = new Uri(_appConfiguration["ServiceAddress:BookingProtoGrpc"]);
            });

            services.AddCors(
               options => options.AddPolicy(
                   _defaultCorsPolicyName,
                   builder => builder
                       .WithOrigins(
                           // App:CorsOrigins in appsettings.json can contain more than one address separated by comma.
                           _appConfiguration["App:CorsOrigins"]
                               .Split(",", StringSplitOptions.RemoveEmptyEntries)
                               .Select(o => o.RemovePostFix("/"))
                               .ToArray()
                       )
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials()
               )
           );

            // Swagger - Enable this line and the related lines in Configure method to enable swagger UI
            ConfigureSwagger(services);

            // Configure Abp and Dependency Injection
            services.AddAbpWithoutCreatingServiceProvider<YootekWebHostModule>(
                // Configure Log4Net logging
                options => options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                    f => f.UseAbpLog4Net().WithConfig(_hostingEnvironment.IsDevelopment()
                        ? "log4net.config"
                        : "log4net.Production.config"
                    )
                )
            );
        }

        public void Configure(IApplicationBuilder app,
            ILoggerFactory loggerFactory)
        {
            app.UseAbp(options => { options.UseAbpRequestLocalization = true; }); // 
            //app.UseHsts();
            //app.UseHttpsRedirection();
            app.UseCors(_defaultCorsPolicyName); // Enable CORS!
            app.UseStaticFiles();

            /**
             * UseStaticFiles
             * Cấu hình static file trên máy chủ IIS
             * build sang máy chủ khác cần comment lại
             */
            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(_appConfiguration["PathStaticFile1"])
            //});
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAbpRequestLocalization();

            app.UseEndpoints(endpoints =>
            {
                // endpoints.MapHub<BusinessHub>("/business");
                endpoints.MapHub<ChatHub>("/messager");
                endpoints.MapHub<AbpCommonHub>("/signalr");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("defaultWithArea", "{area}/{controller=Home}/{action=Index}/{id?}");
                // endpoints.MapHangfireDashboard();

            });

            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger(c => { c.RouteTemplate = "swagger/{documentName}/swagger.json"; });

            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
            app.UseSwaggerUI(options =>
            {
                // specifying the Swagger JSON endpoint.
                options.SwaggerEndpoint($"/swagger/{_apiVersion}/swagger.json", $"Yootek API {_apiVersion}");
                options.IndexStream = () => Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("YOOTEK.Web.Host.wwwroot.swagger.ui.index.html");
                options.DisplayRequestDuration(); // Controls the display of the request duration (in milliseconds) for "Try it out" requests.  
            }); // URL: /swagger

            //app.MapSignalR();
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(_apiVersion, new OpenApiInfo
                {
                    Version = _apiVersion,
                    Title = "YOOTEK API",
                    Description = "YOOTEK",
                    // uncomment if needed TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "YOOTEK",
                        Email = string.Empty,
                        Url = new Uri("https://twitter.com/aspboilerplate"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://github.com/aspnetboilerplate/aspnetboilerplate/blob/dev/LICENSE"),
                    }
                });
                options.DocInclusionPredicate((docName, description) => true);

                // Define the BearerAuth scheme that's in use
                options.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme()
                {
                    Description =
                        "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                //add summaries to swagger
                bool canShowSummaries = _appConfiguration.GetValue<bool>("Swagger:ShowSummaries");
                if (canShowSummaries)
                {
                    var hostXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var hostXmlPath = Path.Combine(AppContext.BaseDirectory, hostXmlFile);
                    options.IncludeXmlComments(hostXmlPath);

                    var applicationXml = $"YOOTEK.Application.xml";
                    var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXml);
                    options.IncludeXmlComments(applicationXmlPath);

                    var webCoreXmlFile = $"YOOTEK.Web.Core.xml";
                    var webCoreXmlPath = Path.Combine(AppContext.BaseDirectory, webCoreXmlFile);
                    options.IncludeXmlComments(webCoreXmlPath);
                }
            });
        }
    }
}
