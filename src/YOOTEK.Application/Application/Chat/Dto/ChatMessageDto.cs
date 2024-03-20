using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities;
using Yootek.Chat;
using Yootek.Organizations.Interface;

namespace Yootek.Chat.Dto
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
        public ChatMessageReadState ReceiverReadState { get; private set; }
        public string Message { get; set; }
        public string FileUrl { get; set; }
        public DateTime CreationTime { get; set; }
        public int? TypeMessage { get; set; }
        public long? MessageRepliedId { get; set; }

        public ChatMessageDto MessageReplied { get; set; }

    }
    public class ChatMessageStatic : ChatMessage, IMayHaveUrban, IMayHaveBuilding
    {
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
    }
}