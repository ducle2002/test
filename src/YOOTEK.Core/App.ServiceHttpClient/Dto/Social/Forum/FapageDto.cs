using System;
using System.Collections.Generic;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Common;

namespace Yootek.App.ServiceHttpClient.Dto.Social.Forum
{
    public enum FanpageState
    {
        Pending = 1,
        Activated = 2,
        Inactivated = 3,
        Hidden = 4,
        Blocked = 5,
    }
    
    public enum Permission
    {
        Admin = 0,
        Censor = 1,
        Follower = 2,
        Member = 3
    }

    public class FanpageDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string? ProfilePictureUrl { get; set; }

        public FanpageState? State { get; set; }
        public string? Bio { get; set; }
        public string? Website { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
    
        public long TotalMembers { get; set; }
        public long TotalPosts { get; set; }
        public bool? IsFollower { get; set; }
        public DateTime? CreationTime { get; set; }
    }
    
    public class CreateFanpageByAdminDto
    {
        public string FanpageName { get; set; }
        public string? FanpageProfilePictureUrl { get; set; }
        public FanpageState? State { get; set; }
        public string? Bio { get; set; }
        public string? Website { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public List<long>? CensorIds { get; set; }
    }
    
    public class FanpageMemberDto : ShortenedUserDto
    {
        public Permission? Permission { get; set; }
        public DateTime? JoinTime { get; set; }
    }
    
    public class GetListMemberOfFanpageDto: CommonInputDto
    {
        public long FanpageId { get; set; }
        public Permission? Permission { get; set; }
    }
    
    public class GetListFanpageByUserDto: CommonInputDto
    {
        public long UserId { get; set; }
        // public Permission? Permission { get; set; }
    }
    
    public class UpdateMemberOfFanpageDto
    {
        public long FanpageId { get; set; }
        public long MemberId { get; set; }
        public Permission Permission { get; set; }
    }
    
    public class UpdateFanpageByAdminDto
    {
        public long Id { get; set; }
        public string? FanpageName { get; set; }
        public string? FanpageProfilePictureUrl { get; set; }
        public FanpageState? State { get; set; }
    }
    
    public class DeleteFanpageDto
    {
        public long Id { get; set; }
    }

    public class DeleteMemberOfFanpageDto
    {
        public long MemberId { get; set; }
        public long FanpageId { get; set; }
    }
    
    public class FanpageUserDto: FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long FanpageId { get; set; }
        public long MemberId { get; set; }
        public Permission? Permission { get; set; }
    }
    
    
}