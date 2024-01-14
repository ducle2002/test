namespace Yootek.Web.Host.Chat
{
    public class SendFriendshipRequestInput
    {
        public long UserId { get; set; }

        public int? TenantId { get; set; }
    }
}