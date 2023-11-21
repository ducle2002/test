using Abp.AutoMapper;
using IMAX.EntityDb;

namespace IMAX.Services.Dto
{
    [AutoMap(typeof(ApartmentType))]
    public class CreateApartmentTypeInput
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }
}
