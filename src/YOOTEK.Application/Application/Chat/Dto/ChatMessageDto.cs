using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using IMAX.Chat;

namespace IMAX.Chat.Dto
{
    [AutoMapFrom(typeof(ChatMessage))]
    public class ChatMessageDto : EntityDto
    {
        public long UserId { get; set; }

        public int? TenantId { get; set; }

        public long TargetUserId { get; set; }

        public int? TargetTenantId { get; set; }

        public ChatSide Side { get; set; }
        public Guid? SharedMessageId { get; set; }
        public ChatMessageReadState ReadState { get; set; }

        public string Message { get; set; }
        public string FileUrl { get; set; }
        public DateTime CreationTime { get; set; }
        public int? TypeMessage { get; set; }
        public long? MessageRepliedId { get; set; }

        public ChatMessageDto MessageReplied { get; set; }

    }
}