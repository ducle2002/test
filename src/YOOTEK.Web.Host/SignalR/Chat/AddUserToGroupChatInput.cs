using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Web.Host.Chat
{
    public class AddUserToGroupChatInput
    {
        public long GroupChatId { get; set; }
        public string GroupName { get; set; }
        public string GroupChatCode { get; set; }
        public List<long> ListFriendIds { get; set; }

    }
}
