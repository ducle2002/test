using Abp.Application.Services.Dto;
using Yootek.Services.Dto;

namespace Yootek.Services.SmartSocial.Advertisements
{
    public class AmenitiesAttributeDto: EntityDto<long>
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public int DataType { get; set; }
        public int InputType { get; set; }
        public int BusinessType { get; set; }
        public string IconUrl { get; set; }
    }
}
