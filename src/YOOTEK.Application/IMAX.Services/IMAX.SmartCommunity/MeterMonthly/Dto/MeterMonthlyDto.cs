using Abp.AutoMapper;
using Abp.Domain.Entities;
using IMAX.EntityDb;
using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using JetBrains.Annotations;

namespace IMAX.Services
{
    [AutoMap(typeof(MeterMonthly))]
    public class MeterMonthlyDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        public long? MeterId { get; set; }
        
        public long? MeterTypeId { get; set; }
        [StringLength(1000)] public DateTime Period { get; set; }
        [StringLength(2000)] public int Value { get; set; }
        public DateTime CreationTime { get; set; }
        public long CreatorUserId { get; set; }
        [CanBeNull] public string ImageUrl { get; set; }
        public string Name { get; set; }
        
    }
}