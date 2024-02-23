

using System.ComponentModel.DataAnnotations;

namespace Yootek.Web.Host.SignalR
{
    public class ProviderSendMessageUserInput
    {
        public int? UserTenantId { get; set; }
        public long? MessageRepliedId { get; set; }
        public long UserId { get; set; }
        public long ProviderId { get; set; }
        public string Message { get; set; }
        public string FileUrl { get; set; }
        public int TypeMessage { get; set; }
        public string ProviderImageUrl { get; set; }
        public string ProviderName { get; set; }
    }

    public class UserSendMessageProviderInput
    {
        public long ProviderId { get; set; }
        public long ProviderUserId { get; set; }
        public int? ProviderTenantId { get; set; }
        public string ProviderImageUrl { get; set; }
        public string ProviderName { get; set; }
        public long? MessageRepliedId { get; set; }
        [Required]
        public string Message { get; set; }
        public string FileUrl { get; set; }
        public int TypeMessage { get; set; }
    }
}
