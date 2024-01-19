using Abp.AutoMapper;
using Yootek.EntityDb;
using Yootek.Organizations.Interface;
using JetBrains.Annotations;
using System;

namespace Yootek.Services.Dto
{
    [AutoMap(typeof(CityVote))]
    public class CreateCityVoteInput : IMayHaveUrban, IMayHaveBuilding
    {
        public string Name { get; set; }
        [CanBeNull] public string Description { get; set; }
        public string Options { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public long? OrganizationUnitId { get; set; }
        public bool? IsShowNumbersVote { get; set; }
        public bool? IsOptionOther { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string? OptionOther { get; set; }
    }
    [AutoMap(typeof(CityVote))]
    public class UpdateCityVoteInput
    {
        public long Id { get; set; }
        public string Name { get; set; }
        [CanBeNull] public string Description { get; set; }
        public string Options { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public long? OrganizationUnitId { get; set; }
        public bool? IsShowNumbersVote { get; set; }
        public bool? IsOptionOther { get; set; }
        public string? OptionOther { get; set; }

    }
}
