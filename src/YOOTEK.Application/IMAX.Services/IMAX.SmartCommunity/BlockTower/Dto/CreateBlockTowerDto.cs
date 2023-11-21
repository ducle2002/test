﻿using Abp.AutoMapper;
using IMAX.EntityDb;
using IMAX.Organizations.Interface;

namespace IMAX.Services
{
    [AutoMap(typeof(BlockTower))]
    public class CreateBlockTowerInput : IMayHaveUrban, IMayHaveBuilding
    {
        public string DisplayName { get; set; }
        public string Code { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string Description { get; set; }
    }
    [AutoMap(typeof(BlockTower))]
    public class CreateBlockTowerByUserInput : IMayHaveUrban, IMayHaveBuilding
    {
        public string DisplayName { get; set; }
        public string Code { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string Description { get; set; }
    }
}