using System;

namespace Yootek.Web.Host.Chat
{
    public class SendChatMessageInput
    {
        public int? TenantId { get; set; }
        public long? MessageRepliedId { get; set; }
        public long UserId { get; set; }

        public string UserName { get; set; }

        public string TenancyName { get; set; }

        public string SenderImageUrl { get; set; }
        public string Message { get; set; }
        public string FileUrl { get; set; }
        public int TypeMessage { get; set; }
    }

    public class DeleteChatMessageInput
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public Guid SharedMessageId { get; set; }
        public long UserId { get; set; }
        public long SenderId { get; set; }
    }


    public class SendGroupChatMessageInput
    {
        public int TypeMessage { get; set; }
        public long RoomId { get; set; }
        public string SenderImageUrl { get; set; }
        public string GroupCode { get; set; }
        public string Message { get; set; }
    }

    public class DeleteGroupChatMessageInput
    {
        public Guid SharedMessageId { get; set; }
        public int? TenantId { get; set; }
        public long UserId { get; set; }
        public long MessageId { get; set; }
        public string UserName { get; set; }
        public string TenancyName { get; set; }
        public string GroupCode { get; set; }
        public long RoomId { get; set; }
    }
}