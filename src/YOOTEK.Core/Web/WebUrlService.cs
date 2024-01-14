using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Abp.Dependency;
using Abp.Extensions;
using Yootek.Configuration;
using Microsoft.Extensions.Configuration;

namespace Yootek.Web
{
    public class WebUrlService : IWebUrlService, ITransientDependency
    {
        public const string TenancyNamePlaceHolder = "{TENANCY_NAME}";
        public string WebSiteRootAddressFormatKey => "App:ClientRootAddress";

        public string ServerRootAddressFormatKey => "App:ServerRootAddress";

        public static string WebSiteRootAddress => "";

        readonly IConfigurationRoot _appConfiguration;

        public WebUrlService(IAppConfigurationAccessor configurationAccessor)
        {
            _appConfiguration = configurationAccessor.Configuration;
        }


        public bool SupportsTenancyNameInUrl
        {
            get
            {
                var siteRootFormat = WebSiteRootAddress;
                return !siteRootFormat.IsNullOrEmpty() && siteRootFormat.Contains(TenancyNamePlaceHolder);
            }
        }


        public string WebSiteRootAddressFormat => _appConfiguration[WebSiteRootAddressFormatKey] ?? "http://localhost:4300/";

        public string ServerRootAddressFormat => _appConfiguration[ServerRootAddressFormatKey] ?? "http://localhost:21021/";

        public string GetSiteRootAddress(string tenancyName = null)
        {
            var siteRootFormat = WebSiteRootAddress;

            if (!siteRootFormat.Contains(TenancyNamePlaceHolder))
            {
                return siteRootFormat;
            }

            if (siteRootFormat.Contains(TenancyNamePlaceHolder + "."))
            {
                siteRootFormat = siteRootFormat.Replace(TenancyNamePlaceHolder + ".", TenancyNamePlaceHolder);
            }

            if (tenancyName.IsNullOrEmpty())
            {
                return siteRootFormat.Replace(TenancyNamePlaceHolder, "");
            }

            return siteRootFormat.Replace(TenancyNamePlaceHolder, tenancyName + ".");
        }

        public string GetServerRootAddress(string tenancyName = null)
        {
            return ReplaceTenancyNameInUrl(ServerRootAddressFormat, tenancyName);
        }

        public List<string> GetRedirectAllowedExternalWebSites()
        {
            var values = _appConfiguration["App:RedirectAllowedExternalWebSites"];
            return values?.Split(',').ToList() ?? new List<string>();
        }

        private string ReplaceTenancyNameInUrl(string siteRootFormat, string tenancyName)
        {
            if (!siteRootFormat.Contains(TenancyNamePlaceHolder))
            {
                return siteRootFormat;
            }

            if (siteRootFormat.Contains(TenancyNamePlaceHolder + "."))
            {
                siteRootFormat = siteRootFormat.Replace(TenancyNamePlaceHolder + ".", TenancyNamePlaceHolder);
            }

            if (tenancyName.IsNullOrEmpty())
            {
                return siteRootFormat.Replace(TenancyNamePlaceHolder, "");
            }

            return siteRootFormat.Replace(TenancyNamePlaceHolder, tenancyName + ".");
        }
    }
}