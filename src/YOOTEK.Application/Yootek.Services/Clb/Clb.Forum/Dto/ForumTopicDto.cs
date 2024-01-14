using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Yootek.Yootek.EntityDb.Forum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yootek.Common;

namespace Yootek.Services
{
    [AutoMap(typeof(ForumTopic))]
    public class ForumTopicDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        [StringLength(256)]
        public string Name { get; set; }
        [StringLength(1000)]
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public long? PostCount { get; set; }
    }
    
    public class CreateForumTopicDto
    {
        public int? TenantId { get; set; }
        [StringLength(256)]
        public string Name { get; set; }
        [StringLength(1000)]
        public string ImageUrl { get; set; }
        public string Description { get; set; }
    }
    
    public class UpdateForumTopicDto
    {
        public long Id { get; set; }
        [StringLength(256)]
        public string Name { get; set; }
        [StringLength(1000)]
        public string ImageUrl { get; set; }
        public string Description { get; set; }
    }

    public class GetAllTopic : CommonInputDto
    {
        
    }
}
