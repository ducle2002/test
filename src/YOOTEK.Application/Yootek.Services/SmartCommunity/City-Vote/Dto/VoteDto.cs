using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using System;
using System.Collections.Generic;
using Abp.Domain.Entities;
using Yootek.Organizations.Interface;

namespace Yootek.Services
{
    [AutoMap(typeof(CityVote))]
    public class CityVoteDto : CityVote, IMayHaveUrban, IMayHaveBuilding
    {
        public List<VoteOption> VoteOptions { get; set; }
        public List<UserVote> UserVotes { get; set; }
        public List<UserVoteDto>?UserVoted { get; set; }
        public long TotalVotes { get; set; }
        public long TotalUsers { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string OrganizationName { get; set; }
        public UserVote UserIsVoted { get; set; }
        public int State { get; set; }
    }

    [AutoMap(typeof(UserVote))]
    public class UserVoteDto : UserVote
    {
        public string VoteOptions { get; set; }
        public string? FullName { get; set; }
        public string? ApartmentCode { get; set; }
        public string? OptionValue { get; set; }
        public string? UrbanName { get; set; }
        public long? UrbanId { get; set; }
        public string? BuildingName { get; set; }
        public long? BuildingId { get; set; }
    }

    public class VoteOption
    {
        public string Id { get; set; }
        public string Option { get; set; }
        public string Value { get; set; }
        public float Percent { get; set; }
        public long CountVote { get; set; }
        public bool IsVoted { get; set; }
        public bool IsOptionOther { get; set; }
        public List<UserVoteDto>? UserVotes { get; set; }
    }

    public class GetAllCityVotes : CommonInputDto
    {
    }

    public class StatisticCityVoteDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
    }

    public class StatisticUserVoteDto
    {
        public long Id { get; set; }
        public string? Comment { get; set; }
        public string OptionVoteId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
    }
    public class UserVoted
    {
        public string? ApartmentCode { get; set; }
        public string? FullName { get; set; }
        public DateTime CreationTime { get; set; }
    }


}
