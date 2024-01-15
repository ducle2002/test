using Abp.AutoMapper;
using Abp.Domain.Entities;
using Yootek.EntityDb;
using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Yootek.Common.Enum;

namespace Yootek.Services
{
    [AutoMap(typeof(MeterType))]
    public class MeterTypeDto : EntityDto<long>
    {
        public int? TenantId { get; set; }
        [StringLength(512)] public string Name { get; set; }
        [StringLength(512)] public string Description { get; set; }
        public DateTime CreationTime { get; set; }
        public long CreatorUserId { get; set; }
        public BillType? BillType { get; set; }
    }
}