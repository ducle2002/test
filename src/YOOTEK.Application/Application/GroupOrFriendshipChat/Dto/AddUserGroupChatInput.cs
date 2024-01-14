using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Friendships.Dto
{
    public class AddUserGroupChatInput
    {
        public long GroupId { get; set; }
        public string GroupName { get; set; }
        public List<FriendAddGroup> MemberShips { get; set; }

    }

    public class FriendAddGroup
    {
        public long FriendId { get; set; }
        public string FriendUserName { get; set; }
        public int? TenantId { get; set; }
    }
}
