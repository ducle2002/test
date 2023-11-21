using Abp.AutoMapper;
using IMAX.EntityDb;

namespace IMAX.Services.Dto
{
    [AutoMap(typeof(ApartmentStatus))]
    public class CreateApartmentStatusInput
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string ColorCode { get; set; }
    }
}
