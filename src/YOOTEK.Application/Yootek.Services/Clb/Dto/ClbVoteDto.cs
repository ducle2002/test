using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using Yootek.Yootek.EntityDb.Clb.Votes;
using Yootek.Organizations.Interface;
using Yootek.Services.Dto;
using JetBrains.Annotations;

namespace Yootek.Yootek.Services.Yootek.Clb.Dto
{
    #region Statistics
    public class StatisticsGetClbCityVoteInput
    {
        public long? OrganizationUnitId { get; set; }
        public int NumberRange { get; set; }
        public QueryCaseCityVote QueryCase { get; set; }
        public FormCityVoteId FormId { get; set; }
    }
    public class GetStatisticClbCityVoteInput
    {
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public int? TenantId { get; set; }
        public QueryCaseDateTime QueryDateTime { get; set; }
        public QueryCaseScope QueryCaseScope { get; set; }
    }
    public class GetStatisticClbUserVoteInput
    {
        public long VoteId { get; set; }
        public QueryCaseDateTime QueryDateTime { get; set; }
    }

    public class ResultStatisticsClbCityVote
    {
        public int CountVotes { get; set; }
        public int CountUserVotes { get; set; }
        public int TotalUsers { get; set; }
        public float PercentUserVotes { get; set; }
    }

    public class DataStatisticClbCityVoteDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int TotalCount { get; set; }
        public int CountVoteComing { get; set; }
        public int CountVoteProgressing { get; set; }
        public int CountVoteFinished { get; set; }
    }

    public class GroupClbCityVoteStatistic
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public List<StatisticClbCityVoteDto> Items { get; set; }
    }

    public class DataStatisticClbUserVoteDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int CountUserVote { get; set; }
    }
    public class GroupClbUserVoteStatistic
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public List<StatisticClbUserVoteDto> Items { get; set; }
    }
    
    #endregion

    #region UserVoteInput
    [AutoMap(typeof(ClbUserVote))]
    public class CreateClbVoteInput
    {
        public long CityVoteId { get; set; }
        public int State { get; set; }
        [StringLength(256)]
        public string OptionVoteId { get; set; }
        public string Comment { get; set; }
    }

    [AutoMap(typeof(ClbUserVote))]
    public class UpdateClbVoteInput
    {
        public long Id { get; set; }
        public int State { get; set; }
        [StringLength(256)]
        public string OptionVoteId { get; set; }
        public string Comment { get; set; }
    }
    

    #endregion

    #region VoteDto
    [AutoMap(typeof(ClbCityVote))]
    public class ClbCityVoteDto : ClbCityVote
    {
        public List<ClbVoteOption> VoteOptions { get; set; }
        public List<ClbUserVote> UserVotes { get; set; }
        public long TotalVotes { get; set; }
        public long TotalUsers { get; set; }
        public string OrganizationName { get; set; }
        public ClbUserVote UserIsVoted { get; set; }
        public int State { get; set; }
    }

    [AutoMap(typeof(ClbUserVote))]
    public class ClbUserVoteDto : ClbUserVote
    {
        public List<ClbVoteOption> VoteOptions { get; set; }
    }

    public class ClbVoteOption
    {
        public string Id { get; set; }
        public string Option { get; set; }
        public string Value { get; set; }
        public float Percent { get; set; }
        public long CountVote { get; set; }
        public bool IsVoted { get; set; }
    }

    public class GetAllClbCityVotes : CommonInputDto
    {
    }

    public class StatisticClbCityVoteDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
    }

    public class StatisticClbUserVoteDto
    {
        public long Id { get; set; }
        public string? Comment { get; set; }
        public string OptionVoteId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
    }
    

    #endregion

    #region VoteInput
    [AutoMap(typeof(ClbCityVote))]
    public class CreateClbCityVoteInput
    {
        public string Name { get; set; }
        [CanBeNull] public string Description { get; set; }
        public string Options { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public bool? IsShowNumbersVote { get; set; }
    }
    [AutoMap(typeof(ClbCityVote))]
    public class UpdateClbCityVoteInput
    {
        public long Id { get; set; }
        public string Name { get; set; }
        [CanBeNull] public string Description { get; set; }
        public string Options { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public long? OrganizationUnitId { get; set; }
        public bool? IsShowNumbersVote { get; set; }
    }
    

    #endregion
}