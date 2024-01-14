using Yootek.Organizations.Interface;
using System;
using System.Collections.Generic;

namespace Yootek.Services.Dto
{
    public enum QueryCaseOrganizationChat
    {
        ByYear = 1,
        ByMonth = 2,
        ByWeek = 3,
        ByDay = 4
    }

    public enum FormOrganizationChatId
    {
        GetAll = 1,
        GetCompleted = 2
    }

    public class StatisticsGetOrganizationChatInput
    {
        public long? OrganizationUnitId { get; set; }
        public int NumberRange { get; set; }
        public QueryCaseOrganizationChat QueryCase { get; set; }
        public FormOrganizationChatId FormId { get; set; }
    }

    public class ResultStatisticsOrganizationChat
    {
        public int CountChatOrganizations { get; set; }
        public int CountChatReflects { get; set; }
    }

    public class GetStatisticOrganizationUnitChatInput : IMayHaveUrban, IMayHaveBuilding
    {
        public QueryCaseDateTime QueryCaseDateTime { get; set; }
        public QueryCaseScope QueryCaseScope { get; set; }
        public int TenantId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
    }

    public class StatisticOrganizationUnitChatDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long TargetUserId { get; set; }
        public DateTime CreationTime { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
    }

    public class GroupOrganizationUnitChatStatistic
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public List<StatisticOrganizationUnitChatDto> Items { get; set; }
    }
    public class DataStatisticOrganizationUnitChat
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public long TotalCount { get; set; }
    }
}
