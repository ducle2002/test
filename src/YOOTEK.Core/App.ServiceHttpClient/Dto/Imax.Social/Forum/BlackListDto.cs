using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using IMAX.Common;
using IMAX.Core.Dto;

namespace IMAX.App.ServiceHttpClient.Dto.Imax.Social.Forum
{
    public enum BlackListAction
    {
        BlockPostComment = 1,
        BlockPostShare = 2,
        BlockPostReact = 3,
        BlockPost = 4,
    }
    
    public enum BlackListScope
    {
        Post = 1,
        User = 2,
        Tenant = 3,
    }
    
    public class BlackListDto : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public List<long> TargetIds { get; set; } = new();
        // public long ObjectIds { get; set; }
        public BlackListAction Action { get; set; }
        public BlackListScope Scope { get; set; } = BlackListScope.Post;
    }
    
    public class CreateBlackListDto 
    {
        public List<long> TargetIds { get; set; } = new();
        // public long ObjectIds { get; set; }
        public BlackListAction Action { get; set; }
        public BlackListScope Scope { get; set; }
    }
    
    public class DeleteBlackListDto
    {
        public long Id { get; set; }
    }
    
    public class GetListBlackListDto : CommonInputDto
    {
        public BlackListAction? BlackListAction { get; set; }
    }
    
    public class UpdateBlackListDto
    {
        public long Id { get; set; }
        public List<long>? TargetIds { get; set; } = new();
        // public long ObjectIds { get; set; }
        public BlackListAction? Action { get; set; }
        public BlackListScope? Scope { get; set; }
    }
    
}