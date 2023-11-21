using Abp.AutoMapper;
using IMAX.Authentication.External;
using System.Collections.Generic;

namespace IMAX.Models.TokenAuth
{
    [AutoMapFrom(typeof(ExternalLoginProviderInfo))]
    public class ExternalLoginProviderInfoModel
    {
        public string Name { get; set; }

        public string ClientId { get; set; }

        public Dictionary<string, string> AdditionalParams { get; set; }
    }
}
