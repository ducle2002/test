using Abp.AutoMapper;
using Abp.Domain.Entities;
using IMAX.EntityDb;

namespace IMAX.Service.Dto
{
    [AutoMap(typeof(ApartmentType))]
    public class ApartmentTypeDto : Entity<long>
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }
}
