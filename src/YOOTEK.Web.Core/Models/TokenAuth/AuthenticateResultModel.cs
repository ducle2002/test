namespace Yootek.Models.TokenAuth
{
    public class AuthenticateResultModel
    {
        public string AccessToken { get; set; }
        public string EncryptedAccessToken { get; set; }
        public int ExpireInSeconds { get; set; }
        public string ThirdAccounts { get; set; }
        public long UserId { get; set; }
        public long TenantId { get; set; }
        public string EmailAddress { get; set; }
        public string MobileConfig { get; set; }
        public string RefreshToken { get; set; }
        public int RefreshTokenExpireInSeconds { get; set; }
    }
}
