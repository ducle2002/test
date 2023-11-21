using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using IMAX.EntityDb;
using IMAX.Organizations.Interface;

namespace IMAX.Services
{
    [AutoMap(typeof(Floor))]
    public class UpdateFloorInput : EntityDto<long>, IMayHaveUrban, IMayHaveBuilding
    {
        public string? Code { get; set; }
        public string DisplayName { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string? Description { get; set; }
    }

    [AutoMap(typeof(Floor))]
    public class UpdateFloorByUserInput : EntityDto<long>, IMayHaveUrban, IMayHaveBuilding
    {
        public string? Code { get; set; }
        public string DisplayName { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string Description { get; set; }
    }
}