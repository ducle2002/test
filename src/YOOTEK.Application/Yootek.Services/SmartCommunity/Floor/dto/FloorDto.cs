using Abp.AutoMapper;
using Abp.Domain.Entities;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using System;

namespace Yootek.Services
{
    [AutoMap(typeof(Floor))]
    public class FloorDto : Entity<long>, IMayHaveUrban, IMayHaveBuilding
    {
        public int? TenantId { get; set; }
        public string DisplayName { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string Description { get; set; }
        public DateTime CreationTime { get; set; }
        public long CreatorUserId { get; set; }
        public string UrbanName { get; set; }
        public string BuildingName { get; set; }
        public string Code { get; set; }
    }
}