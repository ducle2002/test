using Abp.AutoMapper;
using IMAX.EntityDb;
using IMAX.Organizations.Interface;

namespace IMAX.Services
{
    [AutoMap(typeof(Floor))]
    public class CreateFloorInput : IMayHaveUrban, IMayHaveBuilding
    {
        public string Code { get; set; }
        public string DisplayName { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string Description { get; set; }
    }
    [AutoMap(typeof(Floor))]
    public class CreateFloorByUserInput : IMayHaveUrban, IMayHaveBuilding
    {
        public string? Code { get; set; }
        public string DisplayName { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string Description { get; set; }
    }
}