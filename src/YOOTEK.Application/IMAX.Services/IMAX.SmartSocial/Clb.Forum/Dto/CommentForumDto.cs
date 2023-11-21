using Abp.Application.Services.Dto;
using Abp.Domain.Entities.Auditing;
using Abp.AutoMapper;
using IMAX.EntityDb;
using System;

namespace IMAX.Services.Dto
{
    [AutoMap(typeof(ForumComment))]
    public class CommentForumDto: EntityDto<long>, ICreationAudited
    {
        public long? CreatorUserId { get; set; }
        public int? TenantId { get; set; }
        public string Comment { get; set; }
        public string FileUrl { get; set; }
        public long ForumId { get; set; }
        public bool? IsAdmin { get; set; }
        public string CreatorName { get; set; }
        public string CreatorAvatar { get; set; }
        public DateTime CreationTime { get; set; }
    }

    [AutoMap(typeof(ForumComment))]
    public class CreateOrUpdateCommentForumDto: EntityDto<long>
    {
        public int? TenantId { get; set; }
        public string Comment { get; set; }
        public string FileUrl { get; set; }
        public long ForumId { get; set; }
        public bool? IsAdmin { get; set; }
    }
}
