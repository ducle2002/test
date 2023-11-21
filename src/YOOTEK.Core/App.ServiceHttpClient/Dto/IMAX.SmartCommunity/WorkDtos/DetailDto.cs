using IMAX.Common;
using System;

namespace IMAX.App.ServiceHttpClient.Dto.IMAX.SmartCommunity.WorkDtos
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