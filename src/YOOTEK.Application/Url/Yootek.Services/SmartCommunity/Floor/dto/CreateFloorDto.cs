using Abp.AutoMapper;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;

namespace Yootek.Services
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