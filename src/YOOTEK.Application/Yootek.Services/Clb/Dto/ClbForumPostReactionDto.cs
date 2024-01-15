using System;
using Abp.Domain.Entities;
using Yootek.App.ServiceHttpClient.Dto.Social.Forum;
using Yootek.Common;
using Yootek.Yootek.EntityDb.Clb.Projects;
using Yootek.Yootek.EntityDb.Forum;

namespace Yootek.Yootek.Services.Yootek.Clb.Dto
{
    public class PostReactionDto 
    {
        public long? Id { get; set; }
        public int? TenantId { get; set; }
        public long PostId { get; set; }
        public long? CommentId { get; set; }
        public EForumReactionType Type { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public ShortenedUserDto? Creator { get; set; }
    }
    
    public class CreateReactionDto
    {
        public int? TenantId { get; set; }
        public long PostId { get; set; }
        public long? CommentId { get; set; }
        public EForumReactionType Type { get; set; }
    }
    
    public class UpdateReactionDto : Entity<long>
    {
        public EForumReactionType Type { get; set; }
    }
    
    public class GetAllReactionDto : CommonInputDto
    {
        public long PostId { get; set; }
        public long? CommentId { get; set; }
        public EForumReactionType? Type { get; set; }
    }
}