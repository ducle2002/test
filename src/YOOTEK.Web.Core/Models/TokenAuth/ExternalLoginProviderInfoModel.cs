using Abp.AutoMapper;
using Yootek.Authentication.External;
using System.Collections.Generic;

namespace Yootek.Models.TokenAuth
{
    [AutoMapFrom(typeof(ExternalLoginProviderInfo))]
    public class ExternalLoginProviderInfoModel
    {
        public string Name { get; set; }

        public string ClientId { get; set; }

        public Dictionary<string, string> AdditionalParams { get; set; }
    }
}
