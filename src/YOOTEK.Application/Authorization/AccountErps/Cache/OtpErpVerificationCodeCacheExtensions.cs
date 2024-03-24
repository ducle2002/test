using Abp.Runtime.Caching;

namespace Yootek.Account.Cache
{
    public static class OtpErpVerificationCodeCacheExtensions
    {
        public static ITypedCache<string, OtpErpVerificationCodeCacheItem> GetOtpErpVerificationCodeCache(this ICacheManager cacheManager)
        {
            return cacheManager.GetCache<string, OtpErpVerificationCodeCacheItem>(OtpErpVerificationCodeCacheItem.CacheName);
        }
    }
}