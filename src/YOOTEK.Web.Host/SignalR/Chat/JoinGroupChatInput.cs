using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Web.Host.Chat.SignalR
{
    public class JoinGroupChatInput
    {
        public long UserId { get; set; }

        public long SenderId { get; set; }

        public string UserName { get; set; }

        public string GroupName { get; set; }
    }
}
