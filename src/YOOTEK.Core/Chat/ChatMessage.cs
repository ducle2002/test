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
    [Table("AppChatMessages")]
    public class ChatMessage : Entity<long>, IHasCreationTime, IMayHaveTenant
    {

        public const int MaxMessageLength = 4 * 1024; //4KB

        public long UserId { get; set; }

        public int? TenantId { get; set; }

        public long TargetUserId { get; set; }

        public int? TargetTenantId { get; set; }
        public long? MessageRepliedId { get; set; }

        [Required]
        [StringLength(MaxMessageLength)]
        public string Message { get; set; }
        public int? TypeMessage { get; set; }
        public string FileUrl { get; set; }
        public DateTime CreationTime { get; set; }
        public bool? IsOrganizationUnit { get; set; }

        public ChatSide Side { get; set; }

        public ChatMessageReadState ReadState { get; private set; }

        public ChatMessageReadState ReceiverReadState { get; private set; }

        public Guid? SharedMessageId { get; set; }

        public ChatMessage(
            UserIdentifier user,
            UserIdentifier targetUser,
            ChatSide side,
            string message,
            ChatMessageReadState readState,
            Guid sharedMessageId,
            ChatMessageReadState receiverReadState,
            int typeMessage = 1,
            long? messageRepliedId = null,
            bool? isOrganization = null,
            string fileUrl = null
            )
        {
            UserId = user.UserId;
            TenantId = user.TenantId;
            TargetUserId = targetUser.UserId;
            TargetTenantId = targetUser.TenantId;
            Message = message;
            Side = side;
            ReadState = readState;
            SharedMessageId = sharedMessageId;
            ReceiverReadState = receiverReadState;
            TypeMessage = typeMessage;
            MessageRepliedId = messageRepliedId;
            CreationTime = Clock.Now;
            IsOrganizationUnit = isOrganization;
            FileUrl = fileUrl;
        }

        public void ChangeReadState(ChatMessageReadState newState)
        {
            ReadState = newState;
        }

        public ChatMessage()
        {

        }

        public void ChangeReceiverReadState(ChatMessageReadState newState)
        {
            ReceiverReadState = newState;
        }
    }
}
