using Abp;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Chat
{

    [Table("AppGroupMessages")]
    public class GroupMessage : Entity<long>, IHasCreationTime, IMayHaveTenant
    {
        public const int MaxMessageLength = 4 * 1024; //4KB

        public long UserId { get; set; }

        public int? TenantId { get; set; }

        public long GroupId { get; set; }
        public long CreatorUserId { get; set; }

        [Required]
        [StringLength(MaxMessageLength)]
        public string Message { get; set; }
        public int? TypeMessage { get; set; }
        public Guid? ShareMessageId { get; set; }
        public DateTime CreationTime { get; set; }
        public ChatSide Side { get; set; }
        public ChatMessageReadState ReadState { get; set; }

        public GroupMessage(
            UserIdentifier user,
            long groupid,
            ChatSide side,
            string message,
            ChatMessageReadState readState,
            int typeMessage)
        {
            UserId = user.UserId;
            TenantId = user.TenantId;
            GroupId = groupid;
            Message = message;
            Side = side;
            ReadState = readState;
            CreationTime = Clock.Now;
            TypeMessage = typeMessage;
        }

        public void ChangeReadState(ChatMessageReadState newState)
        {
            ReadState = newState;
        }

        public GroupMessage()
        {

        }
    }
}
