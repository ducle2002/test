using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using JetBrains.Annotations;

namespace Yootek.Services
{
    [AutoMap(typeof(BlockTower))]
    public class UpdateBlockTowerInput : EntityDto<long>, IMayHaveUrban, IMayHaveBuilding
    {
        [CanBeNull] public string DisplayName { get; set; }
        [CanBeNull] public string Code { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        [CanBeNull] public string Description { get; set; }
    }

    [AutoMap(typeof(BlockTower))]
    public class UpdateBlockTowerByUserInput : EntityDto<long>, IMayHaveUrban, IMayHaveBuilding
    {
        [CanBeNull] public string DisplayName { get; set; }
        [CanBeNull] public string Code { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        [CanBeNull] public string Description { get; set; }
    }
}
