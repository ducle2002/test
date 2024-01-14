using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Yootek.Chat;
using Yootek.Chat.Cache;
using Yootek.Chat.Dto;
using Yootek.Dto.Interface;
using Yootek.GroupChats;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Friendships.Dto
{
    [AutoMapFrom(typeof(GroupChat), typeof(GroupChatCacheItem))]
    public class GroupChatDto : ChatFriendOrRoomDto
    {
        public string Name { get; set; }
        public string GroupChatCode { get; set; }
        public string GroupImageUrl { get; set; }
        public int MemberNumer { get; set; }
        public GroupMessage LastMessage { get; set; }
        public List<UserGroupChatDto> Members { get; set; }

    }

    [AutoMapFrom(typeof(UserGroupChat))]
    public class UserGroupChatDto 
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
