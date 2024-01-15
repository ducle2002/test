using Yootek.Common;
using System;

namespace Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity.WorkDtos
{
    public class DetailWorkDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public long WorkTypeId { get; set; }
        public string? WorkTypeName { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class WorkStatisticDto
    {
        public long TotalWorks { get; set; }
        public long TotalWorksToDo { get; set; }
        public long TotalWorksDoing { get; set; }
        public long TotalWorksCompleted { get; set; }
        public long TotalWorksOverdue { get; set; }
        public long TotalWorksCompletedOnTime { get; set; }
        public long TotalWorksCompletedLate { get; set; }
        public long TotalworksCancelled { get; set; }
    }

    public class GetWorkStatisticDto
    {
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }
    public class GetWorkStatisticGeneralDto
    {
        public int? UserId { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public QueryCaseWorkStatistics QueryCase { get; set; }
        public int? Status { get; set; }
    }
    public enum QueryCaseWorkStatistics
    {
        ByYear = 1,
        ByMonth = 2,
        ByWeek = 3,
        ByDay = 4,
        ByHours = 5,
        ByStar = 6,
    }
    public class WorkDetainInCreateWorkTypeDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
    }

    public class GetAllWorkDetailDto : CommonInputDto
    {
        public long? WorkTypeId { get; set; }
    }
    public class GetAllWorkDetailNotPagingDto : FilteredInputDto
    {
        public long? WorkTypeId { get; set; }
    }
    public class CreateWorkDetailDto
    {
        public long WorkTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsCreatedQR { get; set; } = false;
    }

    public class UpdateWorkDetailDto
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public long? WorkTypeId { get; set; }
    }

    public class DeleteWorkDetailDto
    {
        public long Id { get; set; }
    }
}