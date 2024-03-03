using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Application.BusinessChat.Input
{
    public class GetUserFriendshipInput
    {
        public string KeywordName { get; set; }
        public long ProviderId { get; set; }
    }

    public class GetUserChatInput
    {
        public int? TenantId { get; set; }
        public long UserId { get; set; }
        public long ProviderId { get; set; }
    }
}
