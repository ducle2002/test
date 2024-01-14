using Abp.Authorization.Users;
using Abp.AutoMapper;
using Yootek.GroupChats;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Chat.Cache
{
    public class UserGroupChatCacheItem
    {
        public long UserId { get; set; }
        public long GroupChatId { get; set; }
        [StringLength(256)]
        public string MemberFullName { get; set; }
        public string MemberUserName { get; set; }
        public int? MemberTenantId { get; set; }
        public string MemberAvatarUrl { get; set; }
        public GroupChatRole Role { get; set; } = GroupChatRole.Member;
        public DateTime CreationTime { get; set; }
        public int? TenantId { get; set; }
        public List<GroupChatCacheItem> GroupChats { get; set; }
    }

    [AutoMapFrom(typeof(GroupChat))]
    public class GroupChatCacheItem
    {
        public int? TenantId { get; set; }

        public string Name { get; set; }

        public string GroupChatCode { get; set; }

        public string GroupImageUrl { get; set; }
        public DateTime CreationTime { get; set; }

    }

}
