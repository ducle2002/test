using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities;
using Yootek.Common;
using Yootek.EntityDb;
using Yootek.Yootek.EntityDb.Forum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Services
{
    [AutoMap(typeof(ForumPost))]
    public class CreateForumPostDto
    {
        public int? TenantId { get; set; }
        [StringLength(1000)]
        public string ThreadTitle { get; set; }
        public int State { get; set; }
        public int Type { get; set; }
        public string Content { get; set; }
        public List<string> LinkUrls { get; set; }
        public List<string> ImageUrls { get; set; }
        [StringLength(1000)]
        public string Tags { get; set; }
        public long? TopicId { get; set; }
        public long? GroupId { get; set; }
    }

    [AutoMap(typeof(ForumPost))]
    public class UpdateForumPostDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        [StringLength(1000)]
        public string ThreadTitle { get; set; }
        public int State { get; set; }
        public int Type { get; set; }
        public string Content { get; set; }
        public List<string> LinkUrls { get; set; }
        public List<string> ImageUrls { get; set; }
        [StringLength(1000)]
        public string Tags { get; set; }
        public long? TopicId { get; set; }
    }

    [AutoMap(typeof(ForumPost))]
    public class ForumPostDto : ForumPost
    {
        public string CreatorName { get; set; }
        public string CreatorAvatar { get; set; }
        public long CommentCount { get; set; }
        public long? LikeCount { get; set; }
        public long? DislikeCount { get; set; }
        public EForumReactionType? ReactionType { get; set; }
    }
}
