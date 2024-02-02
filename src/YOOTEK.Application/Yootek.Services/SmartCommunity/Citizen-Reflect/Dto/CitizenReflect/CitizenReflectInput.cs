using Abp.AutoMapper;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace Yootek.Services
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

    [AutoMap(typeof(CitizenReflect))]
    public class UpdateCitizenReflectInput : EntityDto<long>
    {
        public int? State { get; set; }
        public long[]? ListHandleUserIds { get; set; }
    }
}
