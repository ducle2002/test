#nullable enable
using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Yootek.Common;
using System;
using System.Collections.Generic;

namespace Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity.WorkDtos
{
    public class WorkLogTimeDto : Entity<long>
    {
        public int? TenantId { get; set; }
        public long WorkId { get; set; }
        public string? WorkName { get; set; }
        public long WorkDetailId { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateFinish { get; set; }
        public string? FullName { get; set; }
        public int Status { get; set; }
        public string? Note { get; set; }
        public List<string>? ImageUrls { get; set; }
        public long? WorkTurnId { get; set; }
        public DateTime CreationTime { get; set; }
        public long CreatorUserId { get; set; }
    }
    public class GetAllWorkLogTimeDto : CommonInputDto
    {
        public long? WorkId { get; set; }
        public long? WorkTurnId { get; set; }
        public long? UserId { get; set; }
    }
    public class GetAllWorkTimeSheetQuery
    {
        public int? UserId { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public QueryCaseWorkTimeSheet QueryCase { get; set; }
        public string? StartDate { get; set; }
    }
    public enum QueryCaseWorkTimeSheet
    {
        ByYear = 1,
        ByMonth = 2,
        ByWeek = 3,
    }
    public class GetWorkTimeSheetDto : Entity<long>
    {
        public string? TitleWorkDetail { get; set; }
        public string? TitleWork { get; set; }
        public List<WorkTimeSheetValue>? WorkTimeSheetValues { get; set; }
    }
    public class WorkTimeSheetValue
    {
        public int key { get; set; }
        public string? value { get; set; }
        public long CreatorUserId { get; set; }
        public string? FullName { get; set; }
    }
    public class GetWorkLogTimeByIdDto : Entity<long>
    {
    }
    public class CreateWorkLogTimeDto
    {
        public long WorkId { get; set; }
        public long WorkDetailId { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateFinish { get; set; }
        public int Status { get; set; }
        public string? Note { get; set; }
        public List<string>? ImageUrls { get; set; }
        public long? WorkTurnId { get; set; }
    }
    public class UpdateManyWorkLogTimeDto
    {
        public long WorkTurnId { get; set; }
        public List<long>? ListLogTimeIdsDelete { get; set; }
        public List<CreateWorkLogTimeDto>? ListLogTimeCreate { get; set; }
        public List<UpdateWorkLogTimeDto>? ListLogTimeUpdate { get; set; }
    }
    public class CreateManyWorkLogTimeDto
    {
        public List<CreateWorkLogTimeDto> Items { get; set; }
    }
    public class UpdateWorkLogTimeDto : Entity<long>
    {
        public DateTime? DateStart { get; set; }
        public DateTime? DateFinish { get; set; }
        public int? Status { get; set; }
        public string? Note { get; set; }
        public List<string>? ImageUrls { get; set; }
    }
    public class UpdateStatusWorkLogTimeDto
    {
        public long WorkTurnId { get; set; }
        public long WorkId { get; set; }
        public long WorkDetailId { get; set; }
        public UpdateStatusActionType ActionType { get; set; }
    }
    public enum UpdateStatusActionType
    {
        RECEIVER_COMPLETE = 1,
        SUPERVISOR_COMPLETE = 2,
        SUPERVISOR_DECLINE = 3,
    }
    public class UpdateStatusLogTimeDto
    {
        public long Id { get; set; }
        public UpdateStatusLogActionType ActionType { get; set; }
    }
    public enum UpdateStatusLogActionType
    {
        RECEIVER_COMPLETE = 1,
        SUPERVISOR_COMPLETE = 2,
        SUPERVISOR_DECLINE = 3,
    }
    public class DeleteWorkLogTimeDto : Entity<long>
    {
    }
}
