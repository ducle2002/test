using Abp.AutoMapper;
using Yootek.GroupChats;
using System;
using System.Collections.Generic;

namespace Yootek.Friendships.Dto
{
    public class CreateGroupChatInput
    {

        public string GroupName { get; set; }

        public List<MemberGroupDto> MemberShips { get; set; }

        public string GroupImageUrl { get; set; }

    }

    [AutoMap(typeof(GroupChat))]
    public class UpdateInfoGroupChatInput 
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string GroupChatCode { get; set; }
        public string GroupImageUrl { get; set; }
        public DateTime CreationTime { get; set; }

    }


    public class MemberGroupDto
    {
        public long FriendUserId { get; set; }

        public int? FriendTenantId { get; set; }
    }
}
