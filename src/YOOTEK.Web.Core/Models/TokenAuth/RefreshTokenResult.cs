namespace Yootek.Models.TokenAuth
{
    public class RefreshTokenResult
    {
        public string AccessToken { get; set; }

        public string EncryptedAccessToken { get; set; }

        public int ExpireInSeconds { get; set; }

        public RefreshTokenResult(string accessToken, string encryptedAccessToken, int expireInSeconds)
        {
            AccessToken = accessToken;
            ExpireInSeconds = expireInSeconds;
            EncryptedAccessToken = encryptedAccessToken;
        }

        public RefreshTokenResult()
        {

        }
    }

    public class RefreshTokenModel
    {
        public string Token { get; set; }
        public long? StoreId { get; set; }
        public long? BranchId { get; set; }
    }

    public static class ErpClaimTypes
    {
        /// <summary>
        /// ERPStoreId.
        /// Default: https://yootek.vn/identity/claims/storeid
        /// </summary>
        public static string StoreId = "https://yootek.vn/identity/claims/storeid";
        /// <summary>
        /// ERPBranchId.
        /// Default: https://yootek.vn/identity/claims/branchid
        /// </summary>
        public static string BranchId = "https://yootek.vn/identity/claims/branchid";
    }
}