using System;

namespace Yootek.Account.Cache
{
    [Serializable]
    public class OtpErpVerificationCodeCacheItem
    {
        public const string CacheName = "OtpErpVerificationCodeCache";

        public string Code { get; set; }

        public OtpErpVerificationCodeCacheItem()
        {

        }

        public OtpErpVerificationCodeCacheItem(string code)
        {
            Code = code;
        }
    }
}