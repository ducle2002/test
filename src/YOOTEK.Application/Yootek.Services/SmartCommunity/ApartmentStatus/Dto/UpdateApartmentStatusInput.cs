using Abp.AutoMapper;
using Abp.Domain.Entities;
using Yootek.EntityDb;
using JetBrains.Annotations;

namespace Yootek.Services.Dto
{
    [AutoMap(typeof(ApartmentStatus))]
    public class UpdateApartmentStatusInput : Entity<long>
    {
        [CanBeNull] public string Name { get; set; }
        [CanBeNull] public string Code { get; set; }
        [CanBeNull] public string ColorCode { get; set; }
    }
}
