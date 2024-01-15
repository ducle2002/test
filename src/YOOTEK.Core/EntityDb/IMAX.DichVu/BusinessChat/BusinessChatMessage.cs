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

namespace Yootek.Chat.BusinessChat
{
    [Table("Business.ChatMessages")]
    public class BusinessChatMessage : Entity<long>, IHasCreationTime, IMayHaveTenant
    {
        public const int MaxMessageLength = 4 * 1024; //4KB

        public long UserId { get; set; }

        public int? TenantId { get; set; }
        public long ProviderId { get; set; }
        public long TargetUserId { get; set; }
        public int? TargetTenantId { get; set; }
        public long? MessageRepliedId { get; set; }
        [StringLength(MaxMessageLength)]
        public string Message { get; set; }
        public int? TypeMessage { get; set; }
        public DateTime CreationTime { get; set; }
        public string FileUrl { get; set; }
        public ChatSide Side { get; set; }

        public ChatMessageReadState ReadState { get; private set; }

        public ChatMessageReadState ReceiverReadState { get; private set; }

        public Guid? SharedMessageId { get; set; }

        public BusinessChatMessage(
            UserIdentifier user,
            UserIdentifier targetUser,
            long providerId,
            ChatSide side,
            string message,
            ChatMessageReadState readState,
            Guid sharedMessageId,
            ChatMessageReadState receiverReadState,
            int typeMessage = 1,
            long? messageRepliedId = null,
            string fileUrl = null)
        {
            UserId = user.UserId;
            TenantId = user.TenantId;
            TargetUserId = targetUser.UserId;
            TargetTenantId = targetUser.TenantId;
            ProviderId = providerId;
            Message = message;
            Side = side;
            ReadState = readState;
            SharedMessageId = sharedMessageId;
            ReceiverReadState = receiverReadState;
            TypeMessage = typeMessage;
            MessageRepliedId = messageRepliedId;
            CreationTime = Clock.Now;
            FileUrl = fileUrl;
        }

        public void ChangeReadState(ChatMessageReadState newState)
        {
            ReadState = newState;
        }

        public BusinessChatMessage()
        {

        }

        public void ChangeReceiverReadState(ChatMessageReadState newState)
        {
            ReceiverReadState = newState;
        }
    }
}
