using Abp.Application.Services.Dto;
using Abp.Domain.Entities.Auditing;
using Abp.AutoMapper;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;

namespace Yootek.Services.Dto
{
    [AutoMap(typeof(ForumComment))]
    public class CommentForumDto: EntityDto<long>
    {
        public int? TenantId { get; set; }
        public string Comment { get; set; }
        public List<string> FileUrls { get; set; }
        public List<string> LinkUrls { get; set; }
        public long ForumPostId { get; set; }
        public long? ReplyId { get; set; }
        public string CreatorName { get; set; }
        public string CreatorAvatar { get; set; }
        public long? CreatorUserId {  get; set; }
        public CommentForumDto? ReplyTo { get; set; }
        public DateTime CreationTime { get; set; }
    }

    [AutoMap(typeof(ForumComment))]
    public class CreateOrUpdateCommentForumDto: EntityDto<long>
    {
        public int? TenantId { get; set; }
        public string Comment { get; set; }
        public List<string> FileUrls { get; set; }
        public List<string> ImageUrls { get; set; }
        public long ForumId { get; set; }
    }

    public class CreateForumCommentDto
    {
        public int? TenantId { get; set; }
        public string Comment { get; set; }
        public List<string> FileUrls { get; set; }
        public List<string> LinkUrls { get; set; }
        public long ForumPostId { get; set; }
        public long? ReplyId { get; set; }
    }
    
    public class UpdateForumCommentDto : Entity<long>
    {
        public string Comment { get; set; }
        public List<string> FileUrls { get; set; }
        public List<string> LinkUrls { get; set; }
    }
}
