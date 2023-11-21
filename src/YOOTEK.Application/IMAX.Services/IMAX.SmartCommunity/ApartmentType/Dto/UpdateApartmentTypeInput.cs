using Abp.AutoMapper;
using Abp.Domain.Entities;
using IMAX.EntityDb;
using JetBrains.Annotations;

namespace IMAX.Services.Dto
{
    [AutoMap(typeof(ApartmentType))]
    public class UpdateApartmentTypeInput : Entity<long>
    {
        [CanBeNull] public string Name { get; set; }
        [CanBeNull] public string Code { get; set; }
    }
}
