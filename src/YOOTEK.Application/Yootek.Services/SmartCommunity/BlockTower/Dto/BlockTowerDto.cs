using Abp.AutoMapper;
using Abp.Domain.Entities;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using System;

namespace Yootek.Services
{
    [AutoMap(typeof(BlockTower))]
    public class BlockTowerDto : Entity<long>, IMayHaveBuilding, IMayHaveUrban
    {
        public int? TenantId { get; set; }
        public string DisplayName { get; set; }
        public string Code { get; set; }
        public long? UrbanId { get; set; }
        public string UrbanName { get; set; }
        public long? BuildingId { get; set; }
        public string BuildingName { get; set; }
        public string Description { get; set; }
        public DateTime CreationTime { get; set; }
        public long CreatorUserId { get; set; }
    }
}
