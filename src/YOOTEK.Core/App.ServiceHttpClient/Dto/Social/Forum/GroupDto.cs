using System;
using System.Collections.Generic;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Common;

namespace Yootek.App.ServiceHttpClient.Dto.Social.Forum
{
    public enum GroupState
    {
        PENDING = 1,
        ACTIVATED = 2,
        INACTIVATED = 3,
        HIDDEN = 4,
        BLOCKED = 5,
    }
    
    public class GroupDto: FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public string GroupName { get; set; }
        public string? GroupProfilePictureUrl { get; set; }
        public bool IsPublic { get; set; }
        public GroupState? State { get; set; }
        
        public long TotalMembers { get; set; }
        public long TotalPosts { get; set; }
        public bool? IsMember { get; set; }
        public Permission? Permission { get; set; }
        public ShortenedInviteDto? Invite { get; set; }
    }
    
    public class GroupMemberDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public long GroupId { get; set; }
        public long MemberId { get; set; }
    
        public DateTime CreationTime { get; set; }
        public Permission? Permission { get; set; }
        public GroupState State { get; set; }
        public ShortenedUserDto? User { get; set; }
    }
    
    public class UserToGroupDto : ShortenedUserDto
    {
        public ShortenedInviteDto? Invite { get; set; }
    }
    
    public class CreateGroupByAdminDto
    {
        public long CreatorId { get; set; }
        public string GroupName { get; set; }
        public string? GroupProfilePictureUrl { get; set; }
        public bool IsPublic { get; set; }
        public GroupState? State { get; set; }
        public List<long>? MemberIds { get; set; }
    }
    
    public class GetListGroupByUserDto: CommonInputDto
    {
        public long? UserId { get; set; }
        public Permission? Permission { get; set; }
        public bool? IsMember { get; set; }
    }
    
    public class GetListMemberOfGroupDto: CommonInputDto
    {
        public long GroupId { get; set; }
        public Permission? Permission { get; set; }
        public GroupState? State { get; set; }
    }
    
    public class GetGroupByIdDto
    {
        public long Id { get; set; }
    }
    
    public class GetSocialMediaUserDto : CommonInputDto
    {
        public long Id { get; set; }
        public string? Keyword { get; set; }
    }
    
    public class UpdateGroupByAdminDto
    {
        public long Id { get; set; }
        public string? GroupName { get; set; }
        public string? GroupProfilePictureUrl { get; set; }
        public bool IsPublic { get; set; }
        public GroupState? State { get; set; }
    }
    
    public class UpdateMemberOfGroupDto
    {
        public long MemberId { get; set; }
        public long GroupId { get; set; }
        public Permission? Permission { get; set; }
        public GroupState State { get; set; }
    }
    
    public class DeleteGroupByAdminDto
    {
        public long Id { get; set; }
    }
    
    public class DeleteGroupMemberDto
    {
        public long GroupId { get; set; }
        public long MemberId { get; set; }
    }
    
    public class GroupUser: FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long GroupId { get; set; }
        public long MemberId { get; set; }
        public Permission? Permission { get; set; }
        public GroupState State { get; set; }
    }
}