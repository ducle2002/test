using System.Collections.Generic;

namespace Yootek.Web
{
    public interface IWebUrlService
    {
        string GetSiteRootAddress(string tenancyName = null);

        bool SupportsTenancyNameInUrl { get; }

        string WebSiteRootAddressFormat { get; }

        string ServerRootAddressFormat { get; }

        string GetServerRootAddress(string tenancyName = null);

        List<string> GetRedirectAllowedExternalWebSites();
    }
}
