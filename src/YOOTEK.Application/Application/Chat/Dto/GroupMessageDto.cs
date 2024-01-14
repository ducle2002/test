using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Chat.Dto
{

    [AutoMapFrom(typeof(GroupMessage))]
    public class GroupMessageDto : EntityDto<long>
    {
        public long UserId { get; set; }

        public int? TenantId { get; set; }

        public long GroupId { get; set; }
        public long CreatorUserId { get; set; }
        public string Message { get; set; }
        public int? TypeMessage { get; set; }
        public Guid? ShareMessageId { get; set; }
        public DateTime CreationTime { get; set; }
        public ChatSide Side { get; set; }
        public ChatMessageReadState ReadState { get; set; }
        public int? TargetTenantId { get; set; }
        public string SenderImageUrl { get; set; }

    }
}
