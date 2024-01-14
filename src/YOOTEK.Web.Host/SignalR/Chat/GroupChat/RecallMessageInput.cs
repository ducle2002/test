using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Web.Host.SignalR.Chat.GroupChat
{
    public class RecallMessageInput
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public long RoomId { get; set; }
        public Guid SharedMessageId { get; set; }
    }
}
