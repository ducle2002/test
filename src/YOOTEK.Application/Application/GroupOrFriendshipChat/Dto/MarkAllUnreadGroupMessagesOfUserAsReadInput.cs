
namespace Yootek.Application.RoomOrFriendships.Dto
{
    public class MarkAllUnreadGroupMessagesOfUserAsReadInput
    {
        public long GroupChatId { get; set; }
        public int? TenantId { get; set; }
    }
}
