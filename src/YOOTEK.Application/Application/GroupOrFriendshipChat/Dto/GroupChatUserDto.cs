using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Yootek.GroupChats;
using System;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Friendships.Dto
{

    [AutoMapFrom(typeof(UserGroupChat))]
    public class GroupChatUserDto : EntityDto
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
    }
}
