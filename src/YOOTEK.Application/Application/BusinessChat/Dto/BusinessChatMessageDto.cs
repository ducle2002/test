using Abp.AutoMapper;
using Yootek.Chat;
using Yootek.Chat.BusinessChat;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Application.BusinessChat.Dto
{
    [AutoMapFrom(typeof(BusinessChatMessage))]
    public class BusinessChatMessageDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }

        public int? TenantId { get; set; }
        public long ProviderId { get; set; }
        public long TargetUserId { get; set; }
        public int? TargetTenantId { get; set; }
        public long? MessageRepliedId { get; set; }
        public string FileUrl { get; set; }
        public string Message { get; set; }
        public int? TypeMessage { get; set; }

        public DateTime CreationTime { get; set; }

        public ChatSide Side { get; set; }

        public ChatMessageReadState ReadState { get; private set; }

        public ChatMessageReadState ReceiverReadState { get; private set; }

        public Guid? SharedMessageId { get; set; }


        public BusinessChatMessageDto MessageReplied { get; set; }
    }
}
