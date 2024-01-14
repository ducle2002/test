using Abp.AutoMapper;
using Abp.Domain.Entities;
using Yootek.Yootek.EntityDb.Forum;

namespace Yootek.Yootek.Services.Yootek.Clb.Clb.Forum.Dto
{
    [AutoMap(typeof(Tag))]
    public class CreateTagDto
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
    }

    [AutoMap(typeof(Tag))]
    public class UpdateTagDto : Entity<long>
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
    }
}