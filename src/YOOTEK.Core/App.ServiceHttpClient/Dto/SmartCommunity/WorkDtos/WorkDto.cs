using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Yootek.Common;
using Yootek.EntityDb;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using static Yootek.Common.Enum.CommonENum;

namespace Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity.WorkDtos
{
    public enum Permision
    {
        BOSS = 1,
        EMPLOYEE = 2,
        VIEWER = 3,
    }
    public enum EWorkStatus
    {
        TO_DO = 1,  // cần làm
        DOING = 2,  // đang diễn ra, đang làm 
        OVERDUE = 3,  // quá hạn
        COMPLETED = 4,  // hoàn thành
        CANCELED = 5,  // hủy bỏ, đóng
    }

    // Get All
    public class WorkDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<string> ImageUrls { get; set; }
        public string Note { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateExpected { get; set; }
        public DateTime? DateFinish { get; set; }
        public long? UserId { get; set; }
        public string WorkerName { get; set; }
        public int? Status { get; set; }
        public long? WorkTypeId { get; set; }
        public long CreatorUserId { get; set; }
        public DateTime CreationTime { get; set; }
        public string QrCode { get; set; }
        public QRObjectDto QRObject { get; set; }
        public string QRAction { get; set; }               
    }

    public class WorkExcelDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<string>? ImageUrls { get; set; }
        public string? Note { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateExpected { get; set; }
        public DateTime? DateFinish { get; set; }
        public long? UserId { get; set; }
        public int? Status { get; set; }
        public long? WorkTypeId { get; set; }
        public long CreatorUserId { get; set; }
        public DateTime CreationTime { get; set; }
        public int? Frequency { get; set; }
        public string? FrequencyOption { get; set; }
        public List<WorkDetail>? ListWorkDetails { get; set; }
        public WorkDetailUserDto? CreatorUser { get; set; }
        public List<WorkDetailUserDto>? RecipientUsers { get; set; }
        public List<WorkDetailUserDto>? SupervisorUsers { get; set; }
        public List<long> RecipientIds { get; set; }
        public List<long> SupervisorIds { get; set; }
        public List<WorkLogTime> WorkLogTimes { get; set; }
    }    
    public class WorkDetailExcelDto: WorkDetail
    {
        public int TotalLogTime { get; set; }
        public int LogTimeComplete { get; set; }
    }

    // Get by id
    public class WorkDetailDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<string>? ImageUrls { get; set; }
        public string? Note { get; set; }
        public long? WorkTypeId { get; set; }
        public int? Status { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateExpected { get; set; }
        public DateTime? DateFinish { get; set; }
        public long? WorkCreatorId { get; set; }
        public List<long> RecipientIds { get; set; }
        public List<long> SupervisorIds { get; set; }
        public List<WorkLogTimeDto> WorkLogTimes { get; set; }
        public List<WorkHistoryDto> WorkHistories { get; set; }
        public List<WorkDetail> ListWorkDetail { get; set; }
        public DateTime CreationTime { get; set; }
        public string? QrCode { get; set; }
        public QRObjectDto? QRObject { get; set; }
        public string? QRAction { get; set; }
        public bool? RemindWork { get; set; }
        public int? Frequency { get; set; }
        public string? FrequencyOption { get; set; }
    }

    public class WorkDetailWithUserNameDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<string>? ImageUrls { get; set; }
        public string? Note { get; set; }
        public long? WorkTypeId { get; set; }
        public int? Status { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateExpected { get; set; }
        public DateTime? DateFinish { get; set; }
        public long? WorkCreatorId { get; set; }
        public WorkDetailUserDto CreatorUser { get; set; }
        public List<WorkDetailUserDto> RecipientUsers { get; set; }
        public List<WorkDetailUserDto> SupervisorUsers { get; set; }
        public List<WorkLogTimeDto> WorkLogTimes { get; set; }
        public List<WorkHistoryDto> WorkHistories { get; set; }
        public List<WorkDetail> ListWorkDetail { get; set; }
        public DateTime CreationTime { get; set; }
        public string? QrCode { get; set; }
        public QRObjectDto? QRObject { get; set; }
        public string? QRAction { get; set; }
        public bool? RemindWork { get; set; }
        public int? Frequency { get; set; }
        public string? FrequencyOption { get; set; }
    }

    public class WorkDetailUserDto
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public int? TenantId { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
    }
    public class GetListWorkByRelatedIdDto
    {
        public long RelatedId { get; set; }
        public WorkAssociationType RelationshipType { get; set; }
    }
    public class GetListWorkDto : CommonInputDto
    {
        public int? FormId { get; set; }
        public int? Status { get; set; }
        public long? WorkTypeId { get; set; }
    }
    public class GetExcelWorkDto
    {
        public int? FormId { get; set; }
        public int? Status { get; set; }
        public long? WorkTypeId { get; set; }
        [CanBeNull] public string Keyword { get; set; }
        public SortBy? SortBy { get; set; }
    }
    public class GetAllWorksNotifyQuery
    {
        public QueryCaseWorkNotify QueryCase { get; set; }
    }
    public enum QueryCaseWorkNotify
    {
        ByWorker = 1,
        BySupervisor = 2,
    }
    public class GetAllWorksPlanQuery
    {
        public int? UserId { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public QueryCaseWorkPlan QueryCase { get; set; }
        public string StartDate { get; set; }
    }
    public enum QueryCaseWorkPlan
    {
        ByYear = 1,
        ByMonth = 2,
        ByWeek = 3,
        ByCurrentUser = 4,
    }
    public enum FormIdWork
    {
        ASSIGNED = 1,
        RECEIVED = 2,
        FOLLOW = 3,
        FULLDATA = 4
    }
    public class CreateWorkDto
    {
        public List<long>? RecipientIds { get; set; }
        public List<long>? SupervisorIds { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<string>? ImageUrls { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateExpected { get; set; }
        public string? Note { get; set; }
        public long WorkTypeId { get; set; }
        public List<CreateWorkAssociationDto>? Items { get; set; }
        public bool? RemindWork { get; set; }
        public int? Frequency { get; set; }
        public string? FrequencyOption { get; set; }
    }
    public class CreateWorkAssociationDto
    {
        public long RelatedId { get; set; }
        public WorkAssociationType RelationshipType { get; set; }
    }
    public enum WorkAssociationType
    {
        REFLECT = 1, // phản ánh số
        SERVICE = 2, // dịch vụ 
        OPERATION_LOG = 3, // nhật ký vận hành
        DIGITAL_SERVICES = 4, //Dịch vụ nội khu
    }
    public class UpdateWorkDto
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public List<string>? ImageUrls { get; set; }
        public List<long>? RecipientIds { get; set; }
        public List<long>? SupervisorIds { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateExpected { get; set; }
        public string? Note { get; set; }
        public long? WorkTypeId { get; set; }
        public bool? RemindWork { get; set; }
        public int? Frequency { get; set; }
        public string FrequencyOption { get; set; }
    }
    public class AssignWorkDto
    {
        public long Id { get; set; }
        public List<long> RecipientIds { get; set; }
        public List<long> SupervisorIds { get; set; }
    }
    public class UpdateStateWorkDto
    {
        public long Id { get; set; }
        public TypeActionUpdateStateWork TypeAction { get; set; }
    }
    public class UpdateStateRelateDto
    {
        public long Id { get; set; }
        public WorkAssociationType RelationshipType { get; set; }
    }
    public enum TypeActionUpdateStateWork
    {
        START_DOING = 1,
        COMPLETE = 2,
        CANCEL = 3,
        REOPEN = 4,
    }
    public class DeleteWorkDto
    {
        public long Id { get; set; }
    }

    public class DeleteManyWorkDto
    {
        public List<long> Ids { get; set; }
    }
    public class GetAllWorksPlanDto : Entity<long>
    {
        public string? TitleWork { get; set; }
        public List<WorkPlanValue>? WorkPlanValues { get; set; }
    }
    public class WorkPlanValue
    {
        public int key { get; set; }
        public string value { get; set; }
        public long CreatorUserId { get; set; }
        public string FullName { get; set; }
    }
}
public class GetAllWorksNotifyDto : Entity<long>
{
    public int? TenantId { get; set; }
    public string? Title { get; set; }
    public DateTime? DateStart { get; set; }
    public DateTime? DateExpected { get; set; }
    public DateTime? DateFinish { get; set; }
    public List<long> UserIds { get; set; }
    public EWorkStatus Status { get; set; }
    public long CreatorUserId { get; set; }
    public DateTime CreationTime { get; set; }
    public string? QRAction { get; set; }
    public bool? RemindWork { get; set; }
    public int? Frequency { get; set; }
    public string? FrequencyOption { get; set; }
    public List<WorkFrequency> workFrequencys
    {
        get
        {
            if (!string.IsNullOrEmpty(FrequencyOption))
            {
                return JsonConvert.DeserializeObject<List<WorkFrequency>>(FrequencyOption);
            }
            else return new List<WorkFrequency>();
        }
    }
}
public class WorkFrequency
{
    public int? dayOfWeek { get; set; }
    public string? time { get; set; }
    public int? month { get; set; }
    public int? day { get; set; }
    public string? timeText { get; set; }
}
