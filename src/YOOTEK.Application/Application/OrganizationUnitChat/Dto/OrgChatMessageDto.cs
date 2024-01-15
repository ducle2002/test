using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Yootek.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Abp.Application.Chat.OrganizationUnitChat.Dto
{
    [AutoMapFrom(typeof(ChatMessage))]
    public class OrgChatMessageDto : EntityDto
    {
        public long UserId { get; set; }

        public int? TenantId { get; set; }

        public long TargetUserId { get; set; }

        public int? TargetTenantId { get; set; }

        public ChatSide Side { get; set; }
        public Guid? SharedMessageId { get; set; }
        public ChatMessageReadState ReadState { get; set; }

        public string Message { get; set; }

        public DateTime CreationTime { get; set; }
        public int? TypeMessage { get; set; }
        public long? MessageRepliedId { get; set; }

        public OrgChatMessageDto MessageReplied { get; set; }

    }
}
