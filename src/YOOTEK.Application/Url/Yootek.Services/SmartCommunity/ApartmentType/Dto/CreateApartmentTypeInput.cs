using Abp.AutoMapper;
using Yootek.EntityDb;

namespace Yootek.Services.Dto
{
    [AutoMap(typeof(ApartmentType))]
    public class CreateApartmentTypeInput
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }
}
