using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.GroupChats
{
    [Table("AppUserGroupChats")]
    public class UserGroupChat : Entity<long>, IHasCreationTime, IMayHaveTenant
    {
        public long UserId { get; set; }
        public long GroupChatId { get; set; }
        [StringLength(1000)]
        public string MemberFullName { get; set; }
        [StringLength(1000)]
        public string MemberUserName { get; set; }
        public int? MemberTenantId { get; set; }
        public string MemberAvatarUrl { get; set; }
        public GroupChatRole Role { get; set; }
        public DateTime CreationTime { get; set; }
        public int? TenantId { get; set; }
    }

    public enum GroupChatRole
    {
        Leader,
        Deputy,
        Member
        
    }
}
