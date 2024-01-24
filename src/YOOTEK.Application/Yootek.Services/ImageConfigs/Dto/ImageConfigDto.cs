using Abp.AutoMapper;
using Yootek.Yootek.EntityDb;
using JetBrains.Annotations;

namespace Yootek.Yootek.Services.Yootek.ImageConfigs.Dto
{
    public class GetListImageConfigInput
    {
        public int? TenantId { get; set; }
        public ImageConfigType? Type { get; set; }
        public ImageConfigScope? Scope { get; set; }
    }

    public class UserGetListImageConfigInput
    {
        public ImageConfigType? Type { get; set; }
        public ImageConfigScope? Scope { get; set; }
    }

    public class ImageConfigDto : ImageConfig
    {
    }

    [AutoMapTo(typeof(ImageConfig))]
    public class CreateImageConfigInput
    {
        public int? TenantId { get; set; }
        public ImageConfigType Type { get; set; }
        public ImageConfigScope Scope { get; set; }
        public string ImageUrl { get; set; }
        [CanBeNull] public string Properties { get; set; }
    }

    [AutoMapTo(typeof(ImageConfig))]
    public class UpdateImageConfigInput
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public ImageConfigType Type { get; set; }
        public ImageConfigScope Scope { get; set; }
        public string ImageUrl { get; set; }
        public string Properties { get; set; }
    }
}