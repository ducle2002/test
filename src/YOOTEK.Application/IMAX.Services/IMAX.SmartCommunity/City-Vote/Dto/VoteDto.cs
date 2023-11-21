using Abp.AutoMapper;
using IMAX.Common;
using IMAX.EntityDb;
using System;
using System.Collections.Generic;

namespace IMAX.Services
{
    [AutoMap(typeof(CityVote))]
    public class CityVoteDto : CityVote
    {
        public List<VoteOption> VoteOptions { get; set; }
        public List<UserVote> UserVotes { get; set; }
        public long TotalVotes { get; set; }
        public long TotalUsers { get; set; }
        public string OrganizationName { get; set; }
        public UserVote UserIsVoted { get; set; }
        public int State { get; set; }
    }

    [AutoMap(typeof(UserVote))]
    public class UserVoteDto : UserVote
    {
        public List<VoteOption> VoteOptions { get; set; }
    }

    public class VoteOption
    {
        public string Id { get; set; }
        public string Option { get; set; }
        public string Value { get; set; }
        public float Percent { get; set; }
        public long CountVote { get; set; }
        public bool IsVoted { get; set; }
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
}
