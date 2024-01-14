#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Yootek.Common;
using JetBrains.Annotations;

namespace Yootek.App.ServiceHttpClient.Dto.Business
{
    public class RateEcoFarmDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public long? ItemId { get; set; }
        public long? ProviderId { get; set; }
        public int RatePoint { get; set; }
        public string? FileUrl { get; set; }
        public string? Comment { get; set; }
        public int Type { get; set; }
        [StringLength(256)] public string UserName { get; set; }
        [StringLength(100)] public string? Email { get; set; }
        public string? Avatar { get; set; }
        public long? AnswerRateId { get; set; }
        public long? OrderId { get; set; }
        public long? BookingId { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime CreationTime { get; set; }
        public object? ObjectDto { get; set; }
        public object? TransactionDto { get; set; } 
    }

    public class CountRateEcoFarmDto
    {
        public double RatePoint { get; set; }
        public int CountRate { get; set; }
        public CountRating CountRating { get; set; }
    }

    public class CountRating
    {
        public int One { get; set; }
        public int Two { get; set; }
        public int Three { get; set; }
        public int Four { get; set; }
        public int Five { get; set; }
    }
    
    public class GetListRateEcoFarmsDto: CommonInputDto
    {
        public int? TenantId { get; set; }
        public long? OrderId { get; set; }
        public long? BookingId { get; set; }
        public long? ItemId { get; set; }
        public long? ProviderId { get; set; }
        public long? UserId { get; set; }
        public int? Rating { get; set; }
        public int? OrderBy { get; set; }
        public int? Type { get; set; }
    }
    
    public class GetRateEcoFarmDetailDto: EntityDto<long>
    {
    }
    
    public class CreateRateEcoFarmDto
    {
        public int? TenantId { get; set; }
        public long? ItemId { get; set; }
        public long? ProviderId { get; set; }
        public int? RatePoint { get; set; }
        public string? FileUrl { get; set; }
        public string? Comment { get; set; }
        public int Type { get; set; }
        [StringLength(256)] public string UserName { get; set; }
        [StringLength(100)] public string? Email { get; set; }
        public long? AnswerRateId { get; set; }
        public string? Avatar { get; set; }
        public long? OrderId { get; set; }
        public long? BookingId { get; set; }
    }
    
    public class UpdateRateEcoFarmDto : EntityDto<long>
    {
        public long? ItemId { get; set; }
        public long? ProviderId { get; set; }
        public int? RatePoint { get; set; }
        public string? FileUrl { get; set; }
        public string? Comment { get; set; }
        public int? Type { get; set; }
        [StringLength(256)] public string? UserName { get; set; }
        [StringLength(100)] public string? Email { get; set; }
        public long? AnswerRateId { get; set; }
        public string? Avatar { get; set; }
        public long? OrderId { get; set; }
        public long? BookingId { get; set; }
    }
    
    public class DeleteRateEcoFarmDto : EntityDto<long>
    {
    }
    
    public class DeleteManyRateEcoFarmDto
    {
        public long? OrderId { get; set; }
        public long? BookingId { get; set; }
        public long? ItemId { get; set; }
        public long? ProviderId { get; set; }
        public long? UserId { get; set; }
    }
}