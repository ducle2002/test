using Abp.Dependency;
using Abp.IO.Extensions;
using Abp.Reflection.Extensions;
using Yootek.Web;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Yootek.Emailing
{
    public class EmailTemplateProvider : IEmailTemplateProvider, ITransientDependency
    {
        private readonly IWebUrlService _webUrlService;
        private readonly IConfiguration _configuration;

        public EmailTemplateProvider(
            IWebUrlService webUrlService,
            IConfiguration configuration
            )
        {
            _webUrlService = webUrlService;
            _configuration = configuration;
        }

        public string GetDefaultTemplate(int? tenantId)
        {

            using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.Emailing.EmailTemplates.default.html"))
            {
                var bytes = stream.GetAllBytes();
                var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                //return template.Replace("{EMAIL_LOGO_URL}", GetTenantLogoUrl(tenantId));
                return template;
            }
        }

        public string GetOTPTemplate()
        {

            using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.Emailing.EmailTemplates.otp.html"))
            {
                var bytes = stream.GetAllBytes();
                var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                return template;
            }
        }


        public string GetUserBillTemplate(int? tenantId)
        {

            if (tenantId == 19)
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.Emailing.EmailTemplates.template.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            else if (tenantId == 64)
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.Emailing.EmailTemplates.htc.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            else if (tenantId == 65)
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.Emailing.EmailTemplates.noxh.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            else if (tenantId == 62)
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.Emailing.EmailTemplates.hts.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            else if (tenantId == 43)
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.Emailing.EmailTemplates.ncp.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            else if (tenantId == 63)
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.Emailing.EmailTemplates.vina22.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            else if (tenantId == 80)
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.Emailing.EmailTemplates.thanhbinh11no.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            else if (tenantId == 94)
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.Emailing.EmailTemplates.vinasinco-2.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            else if (tenantId == 97)
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.Emailing.EmailTemplates.hiyori.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            else if (tenantId == 87)
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.Emailing.EmailTemplates.mhomes.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            else if (tenantId == 84)
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.Emailing.EmailTemplates.tanphong.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            else if (tenantId == 82)
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.Emailing.EmailTemplates.ct2b.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            else if (tenantId == 115)
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.Emailing.EmailTemplates.aden.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            else if (tenantId == 114)
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.Emailing.EmailTemplates.trungdo.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            else if (tenantId == 49)
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.Emailing.EmailTemplates.trungdo.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
            else
            {
                using (var stream = typeof(EmailTemplateProvider).GetAssembly().GetManifestResourceStream("YOOTEK.Authorization.Emailing.EmailTemplates.common_bill.html"))
                {
                    var bytes = stream.GetAllBytes();
                    var template = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    return template;
                }
            }
        }

        private string GetTenantLogoUrl(int? tenantId)
        {
            //if (!tenantId.HasValue)
            //{
            //    return _webUrlService.GetSiteRootAddress().EnsureEndsWith('/') + "TenantCustomization/GetTenantLogo";
            //}

            //return _webUrlService.GetSiteRootAddress().EnsureEndsWith('/') + "TenantCustomization/GetTenantLogo?tenantId=" + tenantId.Value;
            return "https://imaxhitech.com/assets/img/Artboard2.png";
        }
    }
}