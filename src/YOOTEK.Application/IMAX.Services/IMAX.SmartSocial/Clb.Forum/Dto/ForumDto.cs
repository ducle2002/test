using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities;
using IMAX.Common;
using IMAX.EntityDb;
using IMAX.IMAX.EntityDb.Forum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAX.Services
{
    [AutoMap(typeof(Forum))]
    public class CreateForumDto 
    {
        public int? TenantId { get; set; }
        [StringLength(1000)]
        public string ThreadTitle { get; set; }
        public int State { get; set; }
        public int? TypeTitle { get; set; }
        public int Type { get; set; }
        public string Content { get; set; }
        public string FileUrl { get; set; }
        [StringLength(1000)]
        public string Tags { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long? TopicId { get; set; }
    }

    [AutoMap(typeof(Forum))]
    public class UpdateForumDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        [StringLength(1000)]
        public string ThreadTitle { get; set; }
        public int State { get; set; }
        public int? TypeTitle { get; set; }
        public int Type { get; set; }
        public string Content { get; set; }
        public string FileUrl { get; set; }
        [StringLength(1000)]
        public string Tags { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long? TopicId { get; set; }
    }

    [AutoMap(typeof(Forum))]
    public class ForumDto : Forum
    {
        public string CreatorName { get; set; }
        public string CreatorAvatar { get; set; }
        public int CommentCount { get; set; }
        public int CountLike { get; set; }
        public bool IsAdminAnswered { get; set; }
    }
}
