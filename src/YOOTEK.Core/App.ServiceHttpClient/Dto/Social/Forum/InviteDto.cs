using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Common;

namespace Yootek.App.ServiceHttpClient.Dto.Social.Forum
{
    public enum InviteState
    {
        Pending =1,
        Accepted =2,
        Rejected =3,
    }

    public enum InviteType
    {
        InviteJoinGroup = 1,
        InviteLikeFanpage = 2,
        RequestJoinGroup = 3,
    }

    public class ShortenedInviteDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public long RequestToId { get; set; }
        public long RequestById { get; set; }
        public InviteType Type { get; set; }
        public InviteState State { get; set; }
    }
    
    public class InviteDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public long RequestToId { get; set; }
        public long RequestById { get; set; }
        public InviteType Type { get; set; }
        public InviteState State { get; set; }
    
        public ShortenedUserDto? Sender { get; set; }
        public ShortenedUserDto? Receiver { get; set; }
        public GroupOrFanpageDto? GroupOrFanpage { get; set; }
        public DateTime? CreationTime { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }
    
    public class GroupOrFanpageDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
    }
    
    public class CreateInviteDto
    {
        public long RequestToId { get; set; }
        public long RequestById { get; set; }
        [Range(1, 3, ErrorMessage = "Type of invite must be 1, 2 or 3")]
        public InviteType Type { get; set; }
        [Range(1, 3, ErrorMessage = "State of invite must be 1, 2 or 3")]
        public InviteState State { get; set; }
    }
    
    public class GetListInviteDto : CommonInputDto
    {
        public long? RequestById { get; set; }
        public long? RequestToId { get; set; }
        public InviteState? State { get; set; }
        public InviteType Type { get; set; }
    }
    
    public class UpdateInviteDto
    {
        public long Id { get; set; }
        public InviteState State { get; set; }
    }
    
    public class DeleteInviteDto
    {
        public long Id { get; set; }
    }
}