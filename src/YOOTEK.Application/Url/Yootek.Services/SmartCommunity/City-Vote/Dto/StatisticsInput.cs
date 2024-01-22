using Yootek.Organizations.Interface;
using System;
using System.Collections.Generic;

namespace Yootek.Services.Dto
{
    public enum QueryCaseCityVote
    {
        ByYear = 1,
        ByMonth = 2,
        ByWeek = 3,
        ByDay = 4
    }

    public enum FormCityVoteId
    {
        GetAll = 1,
        GetCompleted = 2
    }

    public class StatisticsGetCityVoteInput
    {
        public long? OrganizationUnitId { get; set; }
        public int NumberRange { get; set; }
        public QueryCaseCityVote QueryCase { get; set; }
        public FormCityVoteId FormId { get; set; }
    }
    public class GetStatisticCityVoteInput : IMayHaveUrban, IMayHaveBuilding
    {
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public int? TenantId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public QueryCaseDateTime QueryDateTime { get; set; }
        public QueryCaseScope QueryCaseScope { get; set; }
    }
    public class GetStatisticUserVoteInput
    {
        public long VoteId { get; set; }
        public QueryCaseDateTime QueryDateTime { get; set; }
    }

    public enum QueryCaseDateTime
    {
        YEAR = 1,
        QUARTER = 2,
        MONTH = 3,
        WEEK = 4,
        DAY = 5,
        HOUR = 6,
    }
    public enum QueryCaseScope
    {
        TENANT = 1,
        URBAN = 2,
        BUILDING = 3,
    }

    public class ResultStatisticsCityVote
    {
        public int CountVotes { get; set; }
        public int CountUserVotes { get; set; }
        public int TotalUsers { get; set; }
        public float PercentUserVotes { get; set; }
    }

    public class DataStatisticCityVoteDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int TotalCount { get; set; }
        public int CountVoteComing { get; set; }
        public int CountVoteProgressing { get; set; }
        public int CountVoteFinished { get; set; }
    }

    public class GroupCityVoteStatistic
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public List<StatisticCityVoteDto> Items { get; set; }
    }

    public class DataStatisticUserVoteDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int CountUserVote { get; set; }
    }
    public class GroupUserVoteStatistic
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public List<StatisticUserVoteDto> Items { get; set; }
    }
}
