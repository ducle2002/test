using Abp.AutoMapper;
using IMAX.EntityDb;
using IMAX.Organizations.Interface;
using System.ComponentModel.DataAnnotations;

namespace IMAX.Services
{
    [AutoMap(typeof(CitizenReflect))]
    public class CreateCitizenReflectInput : IMustHaveUrban, IMayHaveBuilding
    {
        public string FullName { get; set; }
        public long UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string? AddressFeeder { get; set; }
        [StringLength(2000)]
        public string Data { get; set; }
        [StringLength(2000)]
        public string FileUrl { get; set; }
    }
}
