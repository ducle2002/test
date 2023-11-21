using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using IMAX.App.ServiceHttpClient.Dto.IMAX.SmartCommunity.WorkDtos;
using IMAX.EntityDb;
using IMAX.Organizations.Interface;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IMAX.Services
{
    [AutoMap(typeof(CitizenReflect))]
    public class CitizenReflectDto : CitizenReflect
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string ImageUrl { get; set; }
        public int? CountAllComment { get; set; }
        public string Note { get; set; }
        public string FileOfNote { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string OrganizationUnitName { get; set; }
        public string HandlerName { get; set; }
        [CanBeNull] public string UrbanName { get; set; }
        [CanBeNull] public string BuildingName { get; set; }
        [CanBeNull] public List<WorkDto> Works { get; set; }
    }

    public class SetTimeProcessInput : EntityDto<long>
    {
        public DateTime FinishTime { get; set; }
    }

    [AutoMap(typeof(CitizenReflectComment))]
    public class UserFeedbackCommentDto : CitizenReflectComment
    {
        public string FullName { get; set; }
        public string ImageUrl { get; set; }
        public long CreatorFeedbackId { get; set; }
    }

    public class GroupFeedbackStatistic
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public List<StatisticFeedbackDto> Items { get; set; }
    }


    public class DataStatisticFeedback
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int TotalCount { get; set; }
        public int CountPending { get; set; }
        public int CountHandling { get; set; }
        public int CountCompleted { get; set; }
        public int CountRated { get; set; }
    }
    public class StatisticFeedbackDto : IMayHaveUrban, IMayHaveBuilding
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public string ApartmentCode { get; set; }
        public int? Type { get; set; }
        public int? State { get; set; }
        public DateTime CreationTime { get; set; }
        public long? CreatorUserId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
    }
}
